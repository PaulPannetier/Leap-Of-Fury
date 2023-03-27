#region using

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;

#endregion

public delegate string SerialyseFunction<T>(T obj);
public delegate T DeserialyseFunction<T>(string s);

//Unity

#region Save

public static class Save
{
    /// <summary>
    /// Convert any Serializable object in JSON string.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns> A string represent the object in parameter</returns>
    public static string ConvertObjectToJSONString(object obj) => JsonUtility.ToJson(obj);
    /// <summary>
    /// Convert any string reprensent a Serializable object to the object.
    /// </summary>
    /// <typeparam name="T">The type of the object return</typeparam>
    /// <param name="JSONString">The string represent the object return</param>
    /// <returns> A Serializable object describe by the string in parameter</returns>
    public static T ConvertJSONStringToObject<T>(string JSONString) => JsonUtility.FromJson<T>(JSONString);
    
    /// <summary>
    /// Write in the customer machine a file with the object inside
    /// </summary>
    /// <param name="objToWrite">The object to save</param>
    /// <param name="filename">the save path, begining to the game's folder</param>
    /// <returns> true if the save complete successfully, false overwise</returns>
    public static bool WriteJSONData(object objToWrite, string fileName)
    {
        try
        {
            string s = ConvertObjectToJSONString(objToWrite);
            if (s == "{}")
                return false;
            File.WriteAllText(Application.dataPath + fileName, s);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Write in the customer machine a file with the object inside in an other thread, when finish invoke the callback function with a param : true if the write succeed, false otherwise
    /// </summary>
    /// <param name="objToWrite">The object to save</param>
    /// <param name="filename">the save path, begining to the game's folder</param>
    /// <returns> true if the save complete successfully, false overwise</returns>
    public static void WriteJSONDataMultiThread(object objToWrite, string fileName, Action<bool> callback)
    {
        //Thread thread = new Thread(new ThreadStart(func));
        Thread thread = new Thread(func);
        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
        WriteMultiTreadData data = new WriteMultiTreadData(Application.dataPath + fileName, callback);
        thread.Start(data);

        void func(object rawData)
        {
            WriteMultiTreadData data = (WriteMultiTreadData)rawData;
            try
            {
                string s = ConvertObjectToJSONString(objToWrite);
                if (s == "{}")
                {
                    data.callbackWrite(false);
                    return;
                }
                File.WriteAllText(data.completeFilename, s);
                data.callbackWrite(true);
            }
            catch
            {
                data.callbackWrite(false);
                return;
            }
        }
    }

    /// <typeparam name="T">The object to read's type</typeparam>
    /// <param name="fileName">The path of the file, begining to the game's folder</param>
    /// <param name="objRead"></param>
    /// <returns> true if the function complete successfully, false overwise</returns>
    public static bool ReadJSONData<T>(string fileName, out T objRead)
    {
        try
        {
            string jsonString = File.ReadAllText(Application.dataPath + fileName);
            if (jsonString == "{}")
            {
                objRead = default(T);
                return false;
            }
            objRead = ConvertJSONStringToObject<T>(jsonString);
            return true;
        }
        catch (Exception)
        {
            objRead = default(T);
            return false;
        }        
    }

    /// <typeparam name="T">The object to read's type</typeparam>
    /// <param name="fileName">The path of the file, begining to the game's folder</param>
    /// <param name="objRead"></param>
    /// <returns> true if the function complete successfully, false overwise</returns>
    public static void ReadJSONDataMultiThread<T>(string fileName, Action<bool, T> callback)
    {
        //Thread thread = new Thread(new ThreadStart(func));
        Thread thread = new Thread(func);
        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
        WriteMultiTreadData<T> data = new WriteMultiTreadData<T>(Application.dataPath + fileName, callback);
        thread.Start(data);

        void func(object rawData)
        {
            WriteMultiTreadData<T> data = (WriteMultiTreadData<T>)rawData;
            try
            {
                string jsonString = File.ReadAllText(data.completeFilename);
                if(jsonString == "{}")
                {
                    data.callbackRead.Invoke(false, default(T));
                    return;
                }
                T objRead = ConvertJSONStringToObject<T>(jsonString);
                data.callbackRead(true, objRead);
            }
            catch (Exception)
            {
                data.callbackRead(false, default(T));
            }
        }
    }

    public static void WriteStringMultiThread(string data, string fileName, Action<bool> callback, bool append = true)
    {
        //Thread thread = new Thread(new ThreadStart(func));
        Thread thread = new Thread(func);
        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
        WriteMultiTreadData threadData = new WriteMultiTreadData(Application.dataPath + fileName, callback);
        thread.Start(threadData);

        void func(object rawData)
        {
            WriteMultiTreadData threadData2 = (WriteMultiTreadData)rawData;
            try
            {
                if(append)
                {
                    StreamWriter stream = new StreamWriter(threadData2.completeFilename, true);
                    stream.Write(data);
                    stream.Close();
                }
                else
                {
                    File.WriteAllText(threadData2.completeFilename, data);
                }
                threadData2.callbackWrite(true);
            }
            catch
            {
                threadData2.callbackWrite(false);
                return;
            }
        }
    }

    public static void ReadJSONDataMultiThread(string fileName, Action<bool, string> callback)
    {
        Thread thread = new Thread(func);
        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
        WriteMultiTreadData<string> data = new WriteMultiTreadData<string>(Application.dataPath + fileName, callback);
        thread.Start(data);

        void func(object rawData)
        {
            WriteMultiTreadData<string> data = (WriteMultiTreadData<string>)rawData;
            try
            {
                string jsonString = File.ReadAllText(data.completeFilename);
                data.callbackRead(true, jsonString);
            }
            catch (Exception)
            {
                data.callbackRead(false, string.Empty);
            }
        }
    }

    public static void ImageInPNGFormat(in Color[] pixels, in int w, in int h, string path, string name, in FilterMode filterMode = FilterMode.Point, in TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
    {
        Texture2D texture = GenerateImage(pixels, w, h, filterMode, textureWrapMode);
        File.WriteAllBytes(Application.dataPath + path + name + @".png", texture.EncodeToPNG());
    }

    public static void ImageInPNGFormat(Texture2D texture, string path, string name)
    {
        File.WriteAllBytes(Application.dataPath + path + name + @".png", texture.EncodeToPNG());
    }

    public static void ImageInJPGFormat(in Color[] pixels, in int w, in int h, string path, string name, in FilterMode filterMode = FilterMode.Point, in TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
    {
        Texture2D texture = GenerateImage(pixels, w, h, filterMode, textureWrapMode);
        File.WriteAllBytes(Application.dataPath + path + name + @".jpg", texture.EncodeToJPG());
    }

    public static void ImageInJPGFormat(Texture2D texture, string path, string name)
    {
        File.WriteAllBytes(Application.dataPath + path + name + @".jpg", texture.EncodeToJPG());
    }

    private static Texture2D GenerateImage(in Color[] pixels, in int w, in int h, in FilterMode filterMode = FilterMode.Point, in TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
    {
        Texture2D img = new Texture2D(w, h, TextureFormat.RGBAFloat, false);
        img.SetPixelData(pixels, 0);
        img.filterMode = filterMode;
        img.wrapMode = textureWrapMode;
        img.Apply();
        return img;
    }

    private class WriteMultiTreadData
    {
        public string completeFilename;
        public Action<bool> callbackWrite;

        public WriteMultiTreadData(string completeFilename, Action<bool> callback)
        {
            this.completeFilename = completeFilename;
            this.callbackWrite = callback;
        }
    }

    private class WriteMultiTreadData<T>
    {
        public string completeFilename;
        public Action<bool, T> callbackRead;

        public WriteMultiTreadData(string completeFilename, Action<bool, T> callbackRead)
        {
            this.completeFilename = completeFilename;
            this.callbackRead = callbackRead;
        }
    }
}

#endregion

#region Random

public static class Random
{
    private static System.Random random = new System.Random();
    private static readonly float twoPi = 2f * Mathf.PI;

    #region Seed

    public static void SetSeed(in int seed)
    {
        random = new System.Random(seed);
        Noise2d.Reseed();
    }
    /// <summary>
    /// randomize de seed of the random, allow to have diffenrent random number at each lauch of the game
    /// </summary>
    public static void SetRandomSeed()
    {
        SetSeed((int)DateTime.Now.Ticks);
    }

    #endregion

    #region Random Value and vector

    /// <returns> A random integer between a and b, [|a, b|]</returns>
    public static int Rand(in int a, in int b) => random.Next(a, b + 1);
    /// <returns> A random float between 0 and 1, [0, 1]</returns>
    public static float Rand() => (float)random.Next(int.MaxValue) / (int.MaxValue - 1);
    /// <returns> A random float between a and b, [a, b]</returns>
    public static float Rand(in float a, in float b) => Rand() * Mathf.Abs(b - a) + a;
    /// <returns> A random int between a and b, [|a, b|[</returns>
    public static int RandExclude(in int a, in int b) => random.Next(a, b);
    /// <returns> A random double between a and b, [a, b[</returns>
    public static float RandExclude(in float a, in float b) => (float)random.NextDouble() * (Mathf.Abs(b - a)) + a;
    public static float RandExclude() => (float)random.NextDouble();
    public static float PerlinNoise(in float x, in float y) => Noise2d.Noise(x, y);
    public static Color Color() => new Color(Rand(), Rand(), Rand(), 1f);
    /// <returns> A random color with a random opacity</returns>
    public static Color ColorTransparent() => new Color(Rand(0, 255) / 255f, Rand(0, 255) / 255f, Rand(0, 255) / 255f, (float)random.NextDouble());
    /// <returns> A random Vector2 normalised</returns>
    public static Vector2 Vector2()
    {
        float angle = RandExclude(0f, twoPi);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    /// <returns> A random Vector2 with de magnitude in param</returns>
    public static Vector2 Vector2(in float magnitude)
    {
        float angle = RandExclude(0f, twoPi);
        return new Vector2(magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle));
    }
    /// <returns> A random Vector2 with a randoml magnitude</returns>
    public static Vector2 Vector2(in float minMagnitude, in float maxMagnitude)
    {
        float angle = RandExclude(0f, twoPi);
        float magnitude = Rand(minMagnitude, maxMagnitude);
        return new Vector2(magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle));
    }
    /// <returns> A random Vector3 normalised</returns>
    public static Vector3 Vector3()
    {
        float teta = Rand(0f, twoPi);
        float phi = RandExclude(0f, twoPi);
        return new Vector3(Mathf.Sin(teta) * Mathf.Cos(phi), Mathf.Sin(teta) * Mathf.Sin(phi), Mathf.Cos(teta));
    }
    /// <returns> A random Vector3 with de magnitude in param</returns>
    public static Vector3 Vector3(in float magnitude)
    {
        float teta = Rand(0f, Mathf.PI);
        float phi = RandExclude(0f, twoPi);
        return new Vector3(magnitude * Mathf.Sin(teta) * Mathf.Cos(phi), magnitude * Mathf.Sin(teta) * Mathf.Sin(phi), magnitude * Mathf.Cos(teta));
    }
    /// <returns> A random Vector3 with a random magnitude</returns>
    public static Vector3 Vector3(in float minMagnitude, in float maxMagnitude)
    {
        float teta = Rand(0f, Mathf.PI);
        float phi = RandExclude(0f, twoPi);
        float magnitude = Rand(minMagnitude, maxMagnitude);
        return new Vector3(magnitude * Mathf.Sin(teta) * Mathf.Cos(phi), magnitude * Mathf.Sin(teta) * Mathf.Sin(phi), magnitude * Mathf.Cos(teta));
    }

    public static Vector2 PointInCircle(CircleCollider2D circle) => PointInCircle(circle.transform.position, circle.radius);
    public static Vector2 PointInCircle(Circle circle) => PointInCircle(circle.center, circle.radius);
    public static Vector2 PointInCircle(in Vector2 center, in float radius) => center + Vector2(Rand(0f, radius));

    public static Vector2 PointInRectangle(BoxCollider2D rec) => PointInRectangle(rec.transform.position, rec.size);
    public static Vector2 PointInRectangle(Hitbox rec) => PointInRectangle(rec.center, rec.size);
    public static Vector2 PointInRectangle(in Vector2 center, in Vector2 size)
    {
        return new Vector2(center.x + Rand(-0.5f, 0.5f) * size.x, center.y + Rand(-0.5f, 0.5f) * size.y);
    }

    public static Vector2 PointInCapsule(CapsuleCollider2D capsule) => PointInCapsule(new Capsule(capsule.transform.position, capsule.size, capsule.direction));
    public static Vector2 PointInCapsule(Capsule capsule)
    {
        float area1 = capsule.c1.radius * capsule.c1.radius * Mathf.PI;
        float area2 = capsule.hitbox.size.y * capsule.hitbox.size.x;
        if(Rand(0f, 2f * area1 + area2) <= area2)
        {
            return PointInRectangle(capsule.hitbox.center, capsule.hitbox.size);
        }
        else
        {
            return (Rand(0, 1) == 0 ? capsule.c1.center : capsule.c2.center) + Vector2(Rand(0f, capsule.c1.radius));
        }
    }

    #endregion

    #region Noise

    public static float[,] PerlinNoise(in int width, in int height, in Vector2 scale)
    {
        float[,] res = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                res[x, y] = (PerlinNoise(x / (width * scale.x), y / (height * scale.y)) + 1f) * 0.5f;
            }
        }
        return res;
    }

    public static Array2D<float> RegularNoise(in int width, in int height)
    {
        Array2D<float> res = new Array2D<float>(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                res[x, y] = Rand();
            }
        }
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scale"></param>
    /// <returns>A Worley noise map that can reapte itself infinitely</returns>
    public static Array2D<float> CellsNoise(int width, int height, in Vector2 scale)
    {
        Array2D<float> res = new Array2D<float>(width, height);

        //avec un scale de 1 <=> 1 point par 100 pixels
        int nbPointsX = Mathf.Max(1, ((width * 0.01f) * Mathf.Sqrt(scale.x)).Round());
        int nbPointsY = Mathf.Min(((height * 0.01f) * Mathf.Sqrt(scale.y)).Round());
        Vector2 recSize = new Vector2((float)width / nbPointsX, (float)height / nbPointsY);

        Array2D<Vector2> samplepoints = new Array2D<Vector2>(nbPointsX, nbPointsY);
        Vector2 offset = recSize * 0.5f;
        for (int x = 0; x < samplepoints.width; x++)
        {
            for (int y = 0; y < samplepoints.height; y++)
            {
                samplepoints[x, y] = Random.PointInRectangle(new Vector2(x * recSize.x + offset.x, y * recSize.y + offset.y), recSize);
            }
        }

        Array2D<Vector2> points = new Array2D<Vector2>(3 * nbPointsX, 3 * nbPointsY);
        for (int x = 0; x < points.width; x++)
        {
            for (int y = 0; y < points.height; y++)
            {
                offset = UnityEngine.Vector2.zero;
                if(x < nbPointsX)
                {
                    offset += new Vector2(-nbPointsX * recSize.x, 0f);
                }
                if (x >= 2 * nbPointsX)
                {
                    offset += new Vector2(nbPointsX * recSize.x, 0f);
                }
                if (y < nbPointsY)
                {
                    offset += new Vector2(0f, -nbPointsY * recSize.y);
                }
                if (y >= 2 * nbPointsY)
                {
                    offset += new Vector2(0f, nbPointsY * recSize.y);
                }

                points[x, y] = samplepoints[x % nbPointsX, y % nbPointsY] + offset;
            }
        }
        samplepoints = null;

        int xRec, yRec;
        Vector2 currentPixel;
        float minSqrDist, currentSqrDist;
        float minValue = float.MaxValue, maxValue = float.MinValue;

        for (int x = 0; x < res.width; x++)
        {
            for (int y = 0; y < res.height; y++)
            {
                currentPixel = new Vector2(x, y);
                xRec = Mathf.FloorToInt(x / recSize.x);
                yRec = Mathf.FloorToInt(y / recSize.y);

                minSqrDist = float.MaxValue;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        currentSqrDist = currentPixel.SqrDistance(points[xRec + nbPointsX + i, yRec + nbPointsY + j]);
                        if(currentSqrDist < minSqrDist)
                        {
                            minSqrDist = currentSqrDist;
                        }

                    }
                }

                res[x, y] = Mathf.Sqrt(minSqrDist);
                minValue = Mathf.Min(minValue, res[x, y]);
                maxValue = Mathf.Max(maxValue, res[x, y]);
            }
        }

        for (int x = 0; x < res.width; x++)
        {
            for (int y = 0; y < res.height; y++)
            {
                res[x, y] = Mathf.InverseLerp(minValue, maxValue, res[x, y]);
            }
        }

        return res;
    }

    private static class Noise2d
    {
        private static int[] _permutation;

        private static readonly Vector2[] _gradients;

        static Noise2d()
        {
            CalculatePermutation(out _permutation);
            CalculateGradients(out _gradients);
        }

        private static void CalculatePermutation(out int[] p)
        {
            p = Enumerable.Range(0, 256).ToArray();

            /// shuffle the array
            for (var i = 0; i < p.Length; i++)
            {
                var source = RandExclude(0, p.Length);

                var t = p[i];
                p[i] = p[source];
                p[source] = t;
            }
        }

        /// <summary>
        /// generate a new permutation.
        /// </summary>
        public static void Reseed()
        {
            CalculatePermutation(out _permutation);
        }

        private static void CalculateGradients(out Vector2[] grad)
        {
            grad = new Vector2[256];

            for (var i = 0; i < grad.Length; i++)
            {
                Vector2 gradient;
                do
                {
                    gradient = new Vector2((RandExclude() * 2f - 1f), (RandExclude() * 2f - 1f));
                }
                while (gradient.SqrMagnitude() >= 1);

                gradient.Normalize();

                grad[i] = gradient;
            }
        }

        private static float Drop(float t)
        {
            t = Mathf.Abs(t);
            return 1f - t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Q(in float u, in float v)
        {
            return Drop(u) * Drop(v);
        }

        public static float Noise(in float x, in float y)
        {
            var cell = new Vector2((float)Mathf.Floor(x), (float)Mathf.Floor(y));

            var total = 0f;

            var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            foreach (var n in corners)
            {
                var ij = cell + n;
                var uv = new Vector2(x - ij.x, y - ij.y);

                var index = _permutation[(int)ij.x % _permutation.Length];
                index = _permutation[(index + (int)ij.y) % _permutation.Length];

                var grad = _gradients[index % _gradients.Length];

                total += Q(uv.x, uv.y) * grad.Dot(uv);
            }
            return Mathf.Max(Mathf.Min(total, 1f), -1f);
        }
    }

    #endregion

    #region Proba laws

    //Generer les lois de probas ! fonction non tester
    public static int Bernoulli(in float p) => Rand() <= p ? 1 : 0;
    public static int Binomial(in int n, in int p)
    {
        int count = 0;
        for (int i = 0; i < n; i++)
            count += Bernoulli(p);
        return count;
    }
    public static float Expodential(in float lambda) => (-1f / lambda) * Mathf.Log(Rand());
    public static int Poisson(in float lambda)
    {
        float x = Rand();
        int n = 0;
        while (x > Mathf.Exp(-lambda))
        {
            x *= Rand();
            n++;
        }
        return n;
    }
    public static int Geometric(in float p)
    {
        int count = 0;
        do
        {
            count++;
        } while (Bernoulli(p) == 0);
        return count;
    }
    private static float N01() => Mathf.Sqrt(-2f * Mathf.Log(Rand())) * Mathf.Cos(twoPi * Rand());
    public static float Normal(in float esp, in float sigma) => N01() * sigma + esp;

    #endregion

}

#endregion

#region 2DArray

[Serializable]
public class Array2D<T>
{
    public T[] array;
    public int width, height;

    public Array2D(in int width, in int height)
    {
        this.width = width; this.height = height;
        array = new T[width * height];
    }

    public T this[int line, int column]
    {
        get
        {
            return array[column + line * width];
        }
        set
        {
            array[column + line * width] = value;
        }
    }

    public T[,] ToArray()
    {
        T[,] res = new T[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                res[i, j] = array[j + i * width];
            }
        }
        return res;
    }

    public List<List<T>> ToList()
    {
        List<List<T>> res = new List<List<T>>();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                res[i][j] = array[j + i * width];
            }
        }
        return res;
    }
}

#endregion

#region Useful

public static class Useful
{
    #region Colordeeper/lighter

    /// <summary>
    /// Return a Color deeper than the color in argument
    /// </summary>
    /// <param name="c">The color to change</param>
    /// <param name="percent">le coeff €[0, 1] d'assombrissement</param>
    /// <returns></returns>
    public static Color ColorDeeper(in Color c, float coeff) => new Color(c.r * (1f - coeff), c.g * (1f - coeff), c.b * (1f - coeff), c.a);
    /// <summary>
    /// Return a Color lighter than the color in argument
    /// </summary>
    /// <param name="c">The color to change</param>
    /// <param name="percent">le coeff €[0, 1] de luminosité</param>
    /// <returns></returns>
    public static Color ColorLighter(in Color c, float coeff) => new Color(((1f - c.r) * coeff) + c.r, ((1f - c.g) * coeff) + c.g, ((1f - c.b) * coeff) + c.b, c.a);

    public static Texture2D Lerp(Texture2D A, Texture2D B, float t)
    {
        if (A.width != B.width || A.height != B.height)
        {
            int w = Mathf.Min(A.width, B.width);
            int h = Mathf.Min(A.height, B.height);
            if (A.width < B.width || A.height < B.height)
                A.Resize(w, h);
            if (B.width < A.width || B.height < A.height)
                B.Resize(w, h);
        }
        Texture2D texture = new Texture2D(A.width, A.height);
        for (int x = 0; x < A.width; x++)
        {
            for (int y = 0; y < A.height; y++)
            {
                texture.SetPixel(x, y, Color.Lerp(A.GetPixel(x, y), B.GetPixel(x, y), t));
            }
        }
        return texture;
    }

    #endregion

    #region Vector and Maths

    //Vector2
    public static float SqrDistance(in this Vector2 v, in Vector2 a) => (a.x - v.x) * (a.x - v.x) + (a.y - v.y) * (a.y - v.y);
    public static float Distance(in this Vector2 v, in Vector2 a) => Mathf.Sqrt(v.SqrDistance(a));
    public static bool IsCollinear(this Vector2 a, in Vector2 v) => Mathf.Abs((v.x / a.x) - (v.y / a.y)) < 1e-3f;
    public static Vector3 Cross(in this Vector2 v1, in Vector2 v) => new Vector3(0f, 0f, v1.x * v.y - v1.y * v.x);
    public static float Dot(in this Vector2 v1, in Vector2 v) => v1.x * v.x + v1.y * v.y;
    public static Vector2 Vector2FromAngle(in float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    public static Vector2 Vector2FromAngle(in float angle, in float length) => new Vector2(length * Mathf.Cos(angle), length * Mathf.Sin(angle));


    public static Vector3 ToVector3(in this Vector2 v) => new Vector3(v.x, v.y);
    public static Vector4 ToVector4(in this Vector2 v) => new Vector4(v.x, v.y);
    /// <returns>the orthogonal normalised vector of v</returns>
    public static Vector2 NormalVector(in this Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Epsilon)
        {
            float y = Mathf.Sqrt(1f / (((v.y * v.y) / (v.x * v.x)) + 1f));
            return new Vector2(-v.y * y / v.x, y);
        }
        else if (Mathf.Abs(v.y) > Mathf.Epsilon)
        {
            float x = Mathf.Sqrt(1f / (1f + (v.x * v.x) / (v.y * v.y)));
            return new Vector2(x, -v.x * x / v.y);
        }
        else
        {
            return Vector2.right;//tout les vecteurs de norme 1 marche
        }
    }
    //Vector3
    public static float SqrDistance(in this Vector3 v, in Vector3 a) => (a.x - v.x) * (a.x - v.x) + (a.y - v.y) * (a.y - v.y) + (a.z - v.z) * (a.z - v.z);
    public static float Distance(in this Vector3 v, in Vector3 a) => Mathf.Sqrt(v.SqrDistance(a));
    public static bool IsCollinear(this Vector3 a, in Vector3 b) => Mathf.Abs(b.x / a.x - b.y / a.y) < 0.007f * Mathf.Abs(b.y / a.y) &&
                                                                        Mathf.Abs(b.x / a.x - b.z / a.z) < 0.007f * Mathf.Abs(b.z / a.z) &&
                                                                        Mathf.Abs(b.y / a.y - b.z / a.z) < 0.007f * Mathf.Abs(b.z / a.z);

    public static Vector3 Cross(in this Vector3 v1, in Vector3 v) => new Vector3(v1.y * v.z - v1.z * v.y, v1.z * v.x - v1.x * v.z, v1.x * v.y - v1.y * v.x);
    public static float Dot(in this Vector3 v1, in Vector3 v) => v1.x * v.x + v1.y * v.y + v1.z * v.z;

    public static Vector3 NormalVector(in this Vector3 v1, in Vector3 v) => v1.Cross(v).normalized;
    public static Vector2 ToVector2(in this Vector3 v) => new Vector2(v.x, v.y);
    public static Vector4 ToVector4(in this Vector3 v) => new Vector4(v.x, v.y);
    //Vector4
    public static float SqrDistance(in this Vector4 v, in Vector4 a) => (a.x - v.x) * (a.x - v.x) + (a.y - v.y) * (a.y - v.y) + (a.z - v.z) * (a.z - v.z) + (a.w - v.w) * (a.w - v.w);
    public static float Distance(in this Vector4 v, in Vector4 a) => Mathf.Sqrt(v.SqrDistance(a));
    public static Vector2 ToVector2(in this Vector4 v) => new Vector2(v.x, v.y);
    public static Vector3 ToVector3(in this Vector4 v) => new Vector3(v.x, v.y, v.z);


    public static float ToRad(in float angle) => (angle * Mathf.Deg2Rad) % (2f * Mathf.PI);
    public static float ToDegrees(in float angle) => (angle * Mathf.Rad2Deg) % 360f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">le point de début du vecteur</param>
    /// <param name="b">le point de fin du vecteur</param>
    /// <returns>l'angle en rad entre 0 et 2pi entre le vecteur (1,0) et (b-a) </returns>
    public static float AngleHori(this in Vector2 a, in Vector2 b) => Mathf.Atan2(a.y - b.y, a.x - b.x) + Mathf.PI;
    public static float Angle(this in Vector2 a, in Vector2 b) => ClampModulo(-Mathf.PI, Mathf.PI, AngleHori(Vector2.zero, a) + AngleHori(Vector2.zero, b));
    public static float Sign(this float a) => Mathf.Abs(a) <= Mathf.Epsilon ? 0f : (a > 0f ? 1f : -1f);
    public static int Sign(this int a) => a == 0 ? 0 : (a > 0 ? 1 : -1);

    /// <summary>
    /// Renvoie l'angle minimal entre le segment [ca] et [cb]
    /// </summary>
    public static float Angle(in Vector2 c, in Vector2 a, in Vector2 b)
    {
        float ang1 = AngleHori(c, a);
        float ang2 = AngleHori(c, b);
        float diff = Mathf.Abs(ang1 - ang2);
        return Mathf.Min(diff, 2f * Mathf.PI - diff);
    }

    public static float WrapAngle(float angle) => ClampModulo(0f, 2f * Mathf.PI, angle);

    public static float AngleDist(float a1, float a2)
    {
        a1 = WrapAngle(a1);
        a2 = WrapAngle(a2);
        return Mathf.Min(Mathf.Abs(a1 - a2), Mathf.Abs(Mathf.Abs(a1 - a2) - 2f * Mathf.PI));
    }

    /// <returns> a like a = value % (end -  start) + start, a€[start, end[ /returns>
    public static float ClampModulo(in float start, in float end, in float value)
    {
        if (end < start)
            return ClampModulo(end, start, value);
        if (end - start < Mathf.Epsilon)
            return start;

        if (value < end && value >= start)
            return value;
        else
        {
            float modulo = end - start;
            float result = ((value - start) % modulo) + start;
            if (result >= end)
                return result - modulo;
            if (result < start)
                return result + modulo;
            return result;
        }
    }

    /// <summary>
    /// Renvoie si pour aller de l'angle 1 vers l'angle 2 le plus rapidement il faut tourner à droite ou à gauche, ang€[0, 2pi[
    /// </summary>
    public static void DirectionAngle(in float ang1, in float ang2, out bool right)
    {
        float diff = Mathf.Abs(ang1 - ang2);
        float angMin = Mathf.Min(diff, 2f * Mathf.PI - diff);
        right = Mathf.Abs((ang1 + angMin) % (2f * Mathf.PI) - ang2) < 0.1f;
    }
    /// <summary>
    /// Renvoie la valeur arrondi de n
    /// </summary>
    public static int Round(in this float n) => (n - Mathf.Floor(n)) >= 0.5f ? (int)n + 1 : (int)n;
    /// <summary>
    /// Renvoie la valeur arrondi de n au nombre de décimales en param, ex : Round(51.6854, 2) => 51.69
    /// </summary>
    public static float Round(in this float n, in int nbDecimals)
    {
        float npow = n * Mathf.Pow(10f, nbDecimals);
        return npow - (int)npow >= 0.5f ? (((int)(npow + 1)) / Mathf.Pow(10f, nbDecimals)) : (((int)npow) / Mathf.Pow(10f, nbDecimals));
    }

    /// <summary>
    /// t € [0, 1]
    /// </summary>
    public static int Lerp(in int a, in int b, float t) => (int)(a + (b - a) * t);
    public static float Lerp(in float a, in float b, float t) => a + (b - a) * t;

    public static bool IsOdd(this int number) => Mathf.Abs(number) % 2 == 1;
    public static bool IsEven(this int number) => Mathf.Abs(number) % 2 == 0;

    public static Vector3[] GetVertices(in this Bounds bounds)
    {
        Vector3[] res;
        if (Mathf.Abs(bounds.extents.z) < Mathf.Epsilon)
        {
            res = new Vector3[4]
            {
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f),
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f)
            };
        }
        else
        {
            res = new Vector3[8]
            {
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f, bounds.center.z + bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f, bounds.center.z + bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f, bounds.center.z - bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y + bounds.extents.y * 0.5f, bounds.center.z - bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f, bounds.center.z + bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f, bounds.center.z + bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x - bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f, bounds.center.z - bounds.extents.z * 0.5f),
                new Vector3(bounds.center.x + bounds.extents.x * 0.5f, bounds.center.y - bounds.extents.y * 0.5f, bounds.center.z - bounds.extents.z * 0.5f)
            };
        }
        return res;
    }

    public static bool Contain(in this Bounds b, in Bounds bounds)
    {
        Vector3[] vertices = bounds.GetVertices();
        foreach (Vector3 v in vertices)
        {
            if (!b.Contains(v))
                return false;
        }
        return true;
    }

    public static float NearestFromZero(in float a, in float b) => Mathf.Abs(a) < Mathf.Abs(b) ? a : b;
    public static float FarestFromZero(in float a, in float b) => Mathf.Abs(a) > Mathf.Abs(b) ? a : b;

    public static decimal Sqrt(in decimal x)
    {
        if (x < 0) throw new OverflowException("Cannot calculate square root from a negative number");

        decimal current = (decimal)Math.Sqrt((double)x), previous;
        do
        {
            previous = current;
            if (previous == 0.0M) return 0;
            current = (previous + x / previous) * 0.5m;
        }
        while (Math.Abs(previous - current) > 0.0M);
        return current;
    }

    public static decimal Abs(in decimal x) => x >= 0m ? x : -x;

    public static List<float> Solve2DegresEquation(float a, float b, float c)
    {
        float d = b * b - 4f * a * c;
        if (d < 0f)
            return null;
        if (d >= 0f && d < 1e-5)
            return new List<float>() { -b / (2f * a) };

        d = Mathf.Sqrt(d);
        return new List<float>() { (-b - d) / (2f * a), (-b + d) / (2f * a) };
    }

    public static bool FindARoot(Func<float, float> f, Func<float, float> fPrime, out float root, int maxIter = 50, float accuracy = 1e-5f)
    {
        return FindARoot(f, fPrime, Random.Rand(-100f, 100f), out root, maxIter, accuracy);
    }

    public static bool FindARoot(Func<float, float> f, Func<float, float> fPrime, float x0, out float root, int maxIter = 50, float accuracy = 1e-5f)
    {
        int iter = 0;
        float xk = x0;
        while (Mathf.Abs(f(xk)) > accuracy && iter <= maxIter)
        {
            xk = xk - (f(xk) / fPrime(xk));
            iter++;
        }
        if (iter >= maxIter)
        {
            root = xk;
            return false;
        }
        root = xk;
        return true;
    }

    #endregion

    #region Array

    public static T GetRandom<T>(this T[] array) => array[Random.RandExclude(0, array.Length)];

    public static T[,] CloneArray<T>(this T[,] Array)
    {
        T[,] a = new T[Array.GetLength(0), Array.GetLength(1)];
        for (int l = 0; l < a.GetLength(0); l++)
        {
            for (int c = 0; c < a.GetLength(1); c++)
            {
                a[l, c] = Array[l, c];
            }
        }
        return a;
    }

    /// <summary>
    /// Retourne le sous tableau de Array, cad Array[IndexStart]
    /// </summary>
    /// <param name="indexStart">l'index de la première dimension de Array</param>
    public static T[,,] GetSubArray<T>(this T[,,,] Array, in int indexStart = 0)
    {
        T[,,] a = new T[Array.GetLength(1), Array.GetLength(2), Array.GetLength(3)];
        for (int l = 0; l < a.GetLength(0); l++)
        {
            for (int c = 0; c < a.GetLength(1); c++)
            {
                for (int i = 0; i < a.GetLength(2); i++)
                {
                    a[l, c, i] = Array[indexStart, l, c, i];
                }
            }
        }
        return a;
    }

    public static int GetIndexOf<T>(this T[] arr, T value) where T : IComparable
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].CompareTo(value) == 0)
                return i;
        }
        return -1;
    }

    public static bool Exist<T>(this T[,] tab, int l, int c) => l >= 0 && c >= 0 && l < tab.GetLength(0) && c < tab.GetLength(1);

    public static T[] Merge<T>(this T[] arr, in T[] other)
    {
        T[] res = new T[arr.Length + other.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            res[i] = arr[i];
        }
        for (int i = arr.Length; i < res.Length; i++)
        {
            res[i] = other[i - arr.Length];
        }
        return res;
    }

    public static List<T> Merge<T>(this List<T> lst, List<T> other)
    {
        List<T> res = new List<T>();
        for (int i = 0; i < lst.Count; i++)
        {
            res.Add(lst[i]);
        }
        for (int i = 0; i < other.Count; i++)
        {
            res.Add(other[i]);
        }
        return res;
    }

    #region Show

    public static void ShowArray<T>(this T[] tab, string begMessage = "", string endMessage = "")
    {
        Debug.Log(begMessage + tab.ToString<T>() + endMessage);
    }

    public static string ToString<T>(this T[] arr)
    {
        StringBuilder sb = new StringBuilder("[ ");
        for (int l = 0; l < arr.Length; l++)
        {
            sb.Append(arr[l].ToString());
            sb.Append(", ");
        }
        sb.Remove(sb.Length - 2, 2);
        sb.Append(" ]");
        return sb.ToString();
    }

    public static void ShowArray<T>(this T[,] tab)
    {
        string text = "";
        for (int l = 0; l < tab.GetLength(0); l++)
        {
            text = "[ ";
            for (int c = 0; c < tab.GetLength(1); c++)
            {
                text += tab[l, c].ToString() + ", ";
            }
            text = text.Remove(text.Length - 2, 2);
            text += " ],";
            Debug.Log(text);
        }
    }
    public static void ShowArray<T>(this T[,,] tab)
    {
        string text = "";
        for (int l = 0; l < tab.GetLength(0); l++)
        {
            text += "[ ";
            for (int c = 0; c < tab.GetLength(1); c++)
            {
                text += "[ ";
                for (int i = 0; i < tab.GetLength(2); i++)
                {
                    text += tab[l, c, i].ToString() + ", ";
                }
                text = text.Remove(text.Length - 2, 2);
                text += " ], ";
            }
            text = text.Remove(text.Length - 2, 2);
            text += " ], ";
        }
        text = text.Remove(text.Length - 3, 3);
        text += "]";
        Debug.Log(text);
    }
    public static void ShowArray<T>(this T[,,,] tab)
    {
        string text = "";
        for (int l = 0; l < tab.GetLength(0); l++)
        {
            text += "[ ";
            for (int c = 0; c < tab.GetLength(1); c++)
            {
                text += "[ ";
                for (int i = 0; i < tab.GetLength(2); i++)
                {
                    text += "[ ";
                    for (int j = 0; j < tab.GetLength(3); j++)
                    {
                        text += tab[l, c, i, j].ToString() + ", ";
                    }
                    text = text.Remove(text.Length - 2, 2);
                    text += " ], ";
                }
                text = text.Remove(text.Length - 2, 2);
                text += " ], ";
            }
            text = text.Remove(text.Length - 2, 2);
            text += " ], ";
        }
        text = text.Remove(text.Length - 3, 3);
        text += "]";
        Debug.Log(text);
    }
    public static void ShowArray<T>(this T[,,,,] tab)
    {
        string text = "";
        for (int l = 0; l < tab.GetLength(0); l++)
        {
            text += "[ ";
            for (int c = 0; c < tab.GetLength(1); c++)
            {
                text += "[ ";
                for (int i = 0; i < tab.GetLength(2); i++)
                {
                    text += "[ ";
                    for (int j = 0; j < tab.GetLength(3); j++)
                    {
                        text += "[ ";
                        for (int k = 0; k < tab.GetLength(4); k++)
                        {
                            text += tab[l, c, i, j, k].ToString() + ", ";
                        }
                        text = text.Remove(text.Length - 2, 2);
                        text += " ], ";
                    }
                    text = text.Remove(text.Length - 2, 2);
                    text += " ], ";
                }
                text = text.Remove(text.Length - 2, 2);
                text += " ], ";
            }
            text = text.Remove(text.Length - 2, 2);
            text += " ], ";
        }
        text = text.Remove(text.Length - 3, 3);
        text += "]";
        Debug.Log(text);
    }

    #endregion

    #endregion

    #region Normalise Array

    /// <summary>
    /// Normalise tout les éléments de l'array pour obtenir des valeur entre 0f et 1f, ainse le min de array sera 0f, et le max 1f.
    /// </summary>
    /// <param name="array">The array to normalised</param>
    public static void NormaliseArray(this float[] array)
    {
        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < min)
                min = array[i];
            if (array[i] > max)
                max = array[i];
        }
        float maxMinusMin = max - min;
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = (array[i] - min) / maxMinusMin;
        }
    }

    /// <summary>
    /// Change array like the sum of each element make 1f
    /// </summary>
    /// <param name="array">the array to change, each element must to be positive</param>
    public static void GetProbabilityArray(this float[] array)
    {
        float sum = 0f;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < 0f)
            {
                Debug.LogWarning("Array[" + i + "] must to be positive : " + array[i]);
                return;
            }
            sum += array[i];
        }
        for (int i = 0; i < array.Length; i++)
        {
            array[i] /= sum;
        }
    }
    #endregion

    #region Shuffle

    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = list.Count; i >= 0; i--)
        {
            int j = Random.Rand(0, i);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
    /// <summary>
    /// Shuffle a little bit the list, reproduce approximately the real life
    /// </summary>
    /// <param name="percentage">The percentage to shuffle between 0 and 1</param>
    public static void ShufflePartialy<T>(this List<T> list, in float percentage)
    {
        int nbPermut = (int)(list.Count * percentage);
        for (int i = 0; i < nbPermut; i++)
        {
            int randIndex1 = Random.RandExclude(0, list.Count);
            int randIndex2 = Random.RandExclude(0, list.Count);
            T temp = list[randIndex1];
            list[randIndex1] = list[randIndex2];
            list[randIndex2] = temp;
        }
    }

    #endregion

    #region Parse

    private static string[] letter = new string[36] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    private static string Troncate(string mot)
    {
        string newMot = mot;
        for (int i = 0; i < mot.Length; i++)
        {
            string s = mot.Substring(i, 1);
            if (s == "," || s == ".")
            {
                newMot = newMot.Remove(i, mot.Length - i);
                break;
            }
        }
        return newMot;
    }

    public static int ConvertStringToInt(string number)
    {
        int nb = 0;
        number = Troncate(number);
        for (int i = number.Length - 1; i >= 0; i--)
        {
            string carac = number.Substring(i, 1);
            for (int j = 26; j <= 35; j++)
            {
                if (carac == letter[j])
                {
                    int n = j - 26;
                    nb += n * (int)Mathf.Pow(10, number.Length - 1 - i);
                    break;
                }
            }
        }
        if (number.Substring(0, 1) == "-")
            nb *= -1;

        return nb;
    }

    public static float ConvertStringToFloat(string number)
    {
        float result = 0f;
        string partieEntiere = number;
        string partieDecimal = "";

        int indexComa = 0;
        for (int i = 0; i < number.Length; i++)
        {
            string s = number.Substring(i, 1);
            if (s == "," || s == ".")
            {
                partieDecimal = partieEntiere.Substring(i + 1, partieEntiere.Length - (i + 1));
                partieEntiere = partieEntiere.Remove(i, partieEntiere.Length - i);
                indexComa = i;
                break;
            }
        }
        //part entière
        result = ConvertStringToInt(partieEntiere);
        //part decimal
        for (int i = 0; i < partieDecimal.Length; i++)
        {
            string carac = partieDecimal.Substring(i, 1);
            for (int j = 26; j <= 35; j++)
            {
                if (carac == letter[j])
                {
                    int n = j - 26; //n € {0,1,2,3,4,5,6,7,8,9}
                    result += n * (float)Mathf.Pow(10, -(i + 1));
                    break;
                }
            }
        }
        return result;
    }

    #endregion

    #region List

    public static T GetRandom<T>(this List<T> lst) => lst[Random.RandExclude(0, lst.Count)];

    public static T Last<T>(this List<T> lst) => lst[lst.Count - 1];

    public static void RemoveLast<T>(this List<T> lst) => lst.RemoveAt(lst.Count - 1);

    public static void RemoveBeg<T>(this List<T> lst) => lst.RemoveAt(0);

    /// <summary>
    /// Retourne lst1 union lst2
    /// </summary>
    /// <param name="lst1">La première liste</param>
    /// <param name="lst2">La seconde liste</param>
    /// <param name="doublon">SI on autorise ou pas les doublons</param>
    /// <returns></returns>     
    public static List<T> SumList<T>(this List<T> lst1, in List<T> lst2, bool doublon = false)//pas de doublon par defaut
    {
        return SumList(lst1, lst2);
    }

    public static List<T> SumList<T>(in List<T> lst1, in List<T> lst2, bool doublon = false)//pas de doublon par defaut
    {
        List<T> result = new List<T>();
        foreach (T nb in lst1)
        {
            if (doublon || !result.Contains(nb))
                result.Add(nb);
        }
        foreach (T nb in lst2)
        {
            if (doublon || !result.Contains(nb))
                result.Add(nb);
        }
        return result;
    }

    public static void Add<T>(this List<T> lst1, in List<T> lstToAdd, bool doublon = false)//pas de doublon par defaut
    {
        if (doublon)
        {
            foreach (T element in lstToAdd)
            {
                lst1.Add(element);
            }
        }
        else
        {
            foreach (T element in lstToAdd)
            {
                if (lst1.Contains(element))
                {
                    continue;
                }
                lst1.Add(element);
            }
        }

    }

    public static void Show<T>(this List<T> lst)
    {
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < lst.Count; i++)
        {
            sb.Append((lst[i] == null ? "null" : lst[i].ToString()) + ", ");
        }
        sb.Remove(sb.Length - 2, 2);
        sb.Append("]");
        Debug.Log(sb.ToString());
    }

    #endregion

    #region ConvertStingToArray

    /// <typeparam name="T"></typeparam>
    /// <param name="array">The string to convert</param>
    /// <param name="convertFunction">The conversion function of string to T</param>
    /// <param name="dimension">The dimension of the array create</param>
    /// <returns>An object castable of an array of T with the dimension</returns>
    public static object ConvertStingToArray<T>(string array, DeserialyseFunction<T> convertFunction, out int dimension, in T nullElement = default(T))
    {
        array = array.Replace(" ", "");//on enlève tout les espaces
        int dim = 0;//on calcule la dim
        int compteurDim = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].ToString() == "[")
                compteurDim++;
            if (array[i].ToString() == "]")
                compteurDim--;
            dim = Mathf.Max(dim, compteurDim);
        }
        dimension = dim;
        switch (dim)
        {
            case 1:
                return ConvertStringToArray1(array, convertFunction);
            case 2:
                return ConvertStringToArray2(array, convertFunction, nullElement);
            case 3:
                return ConvertStringToArray3(array, convertFunction, nullElement);
            case 4:
                return ConvertStringToArray4(array, convertFunction, nullElement);
            case 5:
                return ConvertStringToArray5(array, convertFunction, nullElement);
            default:
                throw new Exception("To many dim in " + array + " max dimension is 5");
        }
    }

    private static object ConvertStringToArray1<T>(string tab, DeserialyseFunction<T> f)
    {
        List<string> value = new List<string>();
        string val = "";
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].ToString() == "," || tab[i].ToString() == "]" || tab[i].ToString() == "[")
            {
                if (val != "")
                    value.Add(val);
                val = "";
            }
            else
            {
                val += tab[i].ToString();
            }
        }

        T[] result = new T[value.Count];
        for (int i = 0; i < value.Count; i++)
        {
            result[i] = f(value[i]);
        }
        return result;
    }
    private static object ConvertStringToArray2<T>(string tab, DeserialyseFunction<T> f, in T nullElement)
    {
        //"[[1,2,3,4],[4,5,6]]" va retourné new int[2, 4] { {1, 2, 3, 4}, {4, 5, 6, nullElement} };
        int nbline = -1, nbCol = 0;
        int compteurDim = -1;
        int compteurCol = 0;
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].ToString() == "[")
                compteurDim++;
            if (tab[i].ToString() == "]")
            {
                compteurDim--;
                nbline++;
            }
            if (compteurDim == 1)
            {
                if (tab[i].ToString() == ",")
                {
                    compteurCol++;
                    nbCol = Mathf.Max(nbCol, compteurCol + 1);
                }
            }
            else
            {
                compteurCol = 0;
            }
        }
        T[,] result = new T[nbline, nbCol];//on crée et initialise le tab;
        for (int l = 0; l < result.GetLength(0); l++)
        {
            for (int c = 0; c < result.GetLength(1); c++)
            {
                result[l, c] = nullElement;
            }
        }
        //on remplit le resulat
        compteurDim = -1;
        compteurCol = 0;
        int compteurLine = -2;
        string value = "";
        string text;

        for (int i = 0; i < tab.Length; i++)
        {
            text = tab[i].ToString();
            if (text != "[" && text != "]" && text != ",")
            {
                value += text;
            }
            if (text == "[")
            {
                compteurDim++;
                compteurLine++;
                compteurCol = 0;
            }
            else
            {
                if (text == "]")
                {
                    compteurDim--;
                    if (value != "")
                    {
                        result[compteurLine, compteurCol] = f(value);
                        value = "";
                    }
                }
                else
                {
                    if (text == ",")
                    {
                        if (value != "")
                        {
                            result[compteurLine, compteurCol] = f(value);
                            value = "";
                            compteurCol++;
                        }
                    }
                }
            }
        }

        return result;
    }
    private static object ConvertStringToArray3<T>(string tab, DeserialyseFunction<T> f, in T nullElement)
    {
        int nbDim0 = 1, nbDim1 = 1, nbDim2 = 1;
        int compteurDim0 = 0, compteurDim1 = 0, compteurDim2 = 0;
        int compteurDim = -1;
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].ToString() == "[")
                compteurDim++;
            if (tab[i].ToString() == "]")
                compteurDim--;
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim0++;
                        nbDim0 = Mathf.Max(nbDim0, compteurDim0 + 1);
                    }
                    break;
                case 1:
                    compteurDim2 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim1++;
                        nbDim1 = Mathf.Max(nbDim1, compteurDim1 + 1);
                    }
                    break;
                case 2:
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim2++;
                        nbDim2 = Mathf.Max(nbDim2, compteurDim2 + 1);
                    }
                    break;
                default:
                    break;
            }
        }

        T[,,] result = new T[nbDim0, nbDim1, nbDim2];//on crée et initialise le tab;
        for (int a = 0; a < result.GetLength(0); a++)
        {
            for (int b = 0; b < result.GetLength(1); b++)
            {
                for (int c = 0; c < result.GetLength(2); c++)
                {
                    result[a, b, c] = nullElement;
                }
            }
        }
        compteurDim0 = compteurDim1 = compteurDim2 = 0;
        compteurDim = -1;
        string value = "";
        string text;

        for (int i = 0; i < tab.Length; i++)
        {
            text = tab[i].ToString();
            if (text != "[" && text != "]" && text != ",")
                value += text;
            if (text == "[")
            {
                compteurDim++;
                //compteurDim0++;
            }
            if (text == "]")
            {
                compteurDim--;
                if (value != "")
                {
                    result[compteurDim0, compteurDim1, compteurDim2] = f(value);
                    value = "";
                }
            }
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = 0;
                    if (text == ",")
                    {
                        compteurDim0++;
                    }
                    break;
                case 1:
                    compteurDim2 = 0;
                    if (text == ",")
                    {
                        compteurDim1++;
                    }
                    break;
                case 2:
                    if (text == ",")
                    {
                        result[compteurDim0, compteurDim1, compteurDim2] = f(value);
                        value = "";
                        compteurDim2++;
                    }
                    break;
                default:
                    break;
            }
        }
        return result;
    }
    private static object ConvertStringToArray4<T>(string tab, DeserialyseFunction<T> f, in T nullElement)
    {
        int nbDim0 = 1, nbDim1 = 1, nbDim2 = 1, nbDim3 = 1;
        int compteurDim0 = 0, compteurDim1 = 0, compteurDim2 = 0, compteurDim3 = 0;
        int compteurDim = -1;
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].ToString() == "[")
                compteurDim++;
            if (tab[i].ToString() == "]")
                compteurDim--;
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = compteurDim3 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim0++;
                        nbDim0 = Mathf.Max(nbDim0, compteurDim0 + 1);
                    }
                    break;
                case 1:
                    compteurDim2 = compteurDim3 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim1++;
                        nbDim1 = Mathf.Max(nbDim1, compteurDim1 + 1);
                    }
                    break;
                case 2:
                    compteurDim3 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim2++;
                        nbDim2 = Mathf.Max(nbDim2, compteurDim2 + 1);
                    }
                    break;
                case 3:
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim3++;
                        nbDim3 = Mathf.Max(nbDim3, compteurDim3 + 1);
                    }
                    break;
                default:
                    break;
            }
        }

        T[,,,] result = new T[nbDim0, nbDim1, nbDim2, nbDim3];//on crée et initialise le tab;
        for (int a = 0; a < result.GetLength(0); a++)
        {
            for (int b = 0; b < result.GetLength(1); b++)
            {
                for (int c = 0; c < result.GetLength(2); c++)
                {
                    for (int d = 0; d < result.GetLength(3); d++)
                    {
                        result[a, b, c, d] = nullElement;
                    }
                }
            }
        }
        compteurDim0 = compteurDim1 = compteurDim2 = compteurDim3 = 0;
        compteurDim = -1;
        string value = "";
        string text;

        for (int i = 0; i < tab.Length; i++)
        {
            text = tab[i].ToString();
            if (text != "[" && text != "]" && text != ",")
                value += text;
            if (text == "[")
            {
                compteurDim++;
                //compteurDim0++;
            }
            if (text == "]")
            {
                compteurDim--;
                if (value != "")
                {
                    result[compteurDim0, compteurDim1, compteurDim2, compteurDim3] = f(value);
                    value = "";
                }
            }
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = compteurDim3 = 0;
                    if (text == ",")
                    {
                        compteurDim0++;
                    }
                    break;
                case 1:
                    compteurDim2 = compteurDim3 = 0;
                    if (text == ",")
                    {
                        compteurDim1++;
                    }
                    break;
                case 2:
                    compteurDim3 = 0;
                    if (text == ",")
                    {
                        compteurDim2++;
                    }
                    break;
                case 3:
                    if (text == ",")
                    {
                        result[compteurDim0, compteurDim1, compteurDim2, compteurDim3] = f(value);
                        value = "";
                        compteurDim3++;
                    }
                    break;
                default:
                    break;
            }
        }
        return result;
    }
    private static object ConvertStringToArray5<T>(string tab, DeserialyseFunction<T> f, in T nullElement)
    {
        int nbDim0 = 1, nbDim1 = 1, nbDim2 = 1, nbDim3 = 1, nbDim4 = 1;
        int compteurDim0 = 0, compteurDim1 = 0, compteurDim2 = 0, compteurDim3 = 0, compteurDim4 = 0;
        int compteurDim = -1;
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i].ToString() == "[")
                compteurDim++;
            if (tab[i].ToString() == "]")
                compteurDim--;
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = compteurDim3 = compteurDim4 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim0++;
                        nbDim0 = Mathf.Max(nbDim0, compteurDim0 + 1);
                    }
                    break;
                case 1:
                    compteurDim2 = compteurDim3 = compteurDim4 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim1++;
                        nbDim1 = Mathf.Max(nbDim1, compteurDim1 + 1);
                    }
                    break;
                case 2:
                    compteurDim3 = compteurDim4 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim2++;
                        nbDim2 = Mathf.Max(nbDim2, compteurDim2 + 1);
                    }
                    break;
                case 3:
                    compteurDim4 = 0;
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim3++;
                        nbDim3 = Mathf.Max(nbDim3, compteurDim3 + 1);
                    }
                    break;
                case 4:
                    if (tab[i].ToString() == ",")
                    {
                        compteurDim4++;
                        nbDim4 = Mathf.Max(nbDim4, compteurDim4 + 1);
                    }
                    break;
                default:
                    break;
            }
        }

        T[,,,,] result = new T[nbDim0, nbDim1, nbDim2, nbDim3, nbDim4];//on crée et initialise le tab;
        for (int a = 0; a < result.GetLength(0); a++)
        {
            for (int b = 0; b < result.GetLength(1); b++)
            {
                for (int c = 0; c < result.GetLength(2); c++)
                {
                    for (int d = 0; d < result.GetLength(3); d++)
                    {
                        for (int e = 0; e < result.GetLength(4); e++)
                        {
                            result[a, b, c, d, e] = nullElement;
                        }
                    }
                }
            }
        }
        compteurDim0 = compteurDim1 = compteurDim2 = compteurDim3 = compteurDim4 = 0;
        compteurDim = -1;
        string value = "";
        string text;

        for (int i = 0; i < tab.Length; i++)
        {
            text = tab[i].ToString();
            if (text != "[" && text != "]" && text != ",")
                value += text;
            if (text == "[")
            {
                compteurDim++;
                //compteurDim0++;
            }
            if (text == "]")
            {
                compteurDim--;
                if (value != "")
                {
                    result[compteurDim0, compteurDim1, compteurDim2, compteurDim3, compteurDim4] = f(value);
                    value = "";
                }
            }
            switch (compteurDim)
            {
                case 0:
                    compteurDim1 = compteurDim2 = compteurDim3 = compteurDim4 = 0;
                    if (text == ",")
                    {
                        compteurDim0++;
                    }
                    break;
                case 1:
                    compteurDim2 = compteurDim3 = compteurDim4 = 0;
                    if (text == ",")
                    {
                        compteurDim1++;
                    }
                    break;
                case 2:
                    compteurDim3 = compteurDim4 = 0;
                    if (text == ",")
                    {
                        compteurDim2++;
                    }
                    break;
                case 3:
                    compteurDim4 = 0;
                    if (text == ",")
                    {
                        compteurDim3++;
                    }
                    break;
                case 4:
                    if (text == ",")
                    {
                        result[compteurDim0, compteurDim1, compteurDim2, compteurDim3, compteurDim4] = f(value);
                        value = "";
                        compteurDim4++;
                    }
                    break;
                default:
                    break;
            }
        }
        return result;
    }
    #endregion

    #region ConvertArrayToString

    public static string ConvertArrayToString<T>(in object array, in int dimension)
    {
        return ConvertArrayToString<T>(array, dimension, ToString);
    }
    private static string ToString<T>(T obj) => obj.ToString();

    public static string ConvertArrayToString<T>(in object array, in int dimension, SerialyseFunction<T> convertFunction)
    {
        switch (dimension)
        {
            case 1:
                return ConvertArrayToString1((T[])array, convertFunction);
            case 2:
                return ConvertArrayToString2((T[,])array, convertFunction);
            case 3:
                return ConvertArrayToString3((T[,,])array, convertFunction);
            case 4:
                return ConvertArrayToString4((T[,,,])array, convertFunction);
            case 5:
                return ConvertArrayToString5((T[,,,,])array, convertFunction);
            default:
                throw new Exception("Too many dimension in ConvertArrayToString, maximun 5");
        }
    }

    private static string ConvertArrayToString1<T>(in T[] array, SerialyseFunction<T> convertFunction)
    {
        string result = "[";
        for (int i = 0; i < array.Length; i++)
        {
            result += convertFunction(array[i]) + ",";
        }
        result = result.Remove(result.Length - 1, 1) + "]";
        return result;
    }
    private static string ConvertArrayToString2<T>(in T[,] array, SerialyseFunction<T> convertFunction)
    {
        string result = "[";
        for (int l = 0; l < array.GetLength(0); l++)
        {
            result += "[";
            for (int c = 0; c < array.GetLength(1); c++)
            {
                result += convertFunction(array[l, c]) + ",";
            }
            result = result.Remove(result.Length - 1, 1) + "]";
        }
        result = result.Remove(result.Length - 1, 1) + "]";
        return result;
    }
    private static string ConvertArrayToString3<T>(in T[,,] array, SerialyseFunction<T> convertFunction)
    {
        string result = "";
        for (int l = 0; l < array.GetLength(0); l++)
        {
            result += "[";
            for (int c = 0; c < array.GetLength(1); c++)
            {
                result += "[";
                for (int i = 0; i < array.GetLength(2); i++)
                {
                    result += convertFunction(array[l, c, i]) + ",";
                }
                result = result.Remove(result.Length - 1, 1) + "]";
            }
            result = result.Remove(result.Length - 1, 1) + "]";
        }
        result = result.Remove(result.Length - 1, 1) + "]";
        return result;
    }
    private static string ConvertArrayToString4<T>(in T[,,,] array, SerialyseFunction<T> convertFunction)
    {
        string result = "";
        for (int l = 0; l < array.GetLength(0); l++)
        {
            result += "[";
            for (int c = 0; c < array.GetLength(1); c++)
            {
                result += "[";
                for (int i = 0; i < array.GetLength(2); i++)
                {
                    result += "[";
                    for (int j = 0; j < array.GetLength(3); j++)
                    {
                        result += convertFunction(array[l, c, i, j]) + ",";
                    }
                    result = result.Remove(result.Length - 1, 1) + "]";
                }
                result = result.Remove(result.Length - 1, 1) + "]";
            }
            result = result.Remove(result.Length - 1, 1) + "]";
        }
        result = result.Remove(result.Length - 1, 1) + "]";
        return result;
    }
    private static string ConvertArrayToString5<T>(in T[,,,,] array, SerialyseFunction<T> convertFunction)
    {
        string result = "";
        for (int l = 0; l < array.GetLength(0); l++)
        {
            result += "[";
            for (int c = 0; c < array.GetLength(1); c++)
            {
                result += "[";
                for (int i = 0; i < array.GetLength(2); i++)
                {
                    result += "[";
                    for (int j = 0; j < array.GetLength(3); j++)
                    {
                        result += "[";
                        for (int k = 0; k < array.GetLength(4); k++)
                        {
                            result += convertFunction(array[l, c, i, j, k]) + ",";
                        }
                        result = result.Remove(result.Length - 1, 1) + "]";
                    }
                    result = result.Remove(result.Length - 1, 1) + "]";
                }
                result = result.Remove(result.Length - 1, 1) + "]";
            }
            result = result.Remove(result.Length - 1, 1) + "]";
        }
        result = result.Remove(result.Length - 1, 1) + "]";
        return result;
    }
    #endregion

    #region Integrate

    /// <summary>
    /// Rerturn the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static float Integratef(Func<float, float> f, in float a, in float b, in float samplePerUnit = 5f)
    {
        if (Mathf.Abs(a - b) < Mathf.Epsilon || samplePerUnit <= 0f)
            return 0f;
        if (a > b)
            return -Integratef(f, b, a, samplePerUnit);

        float I = 0f;

        int nbSub = Mathf.Max(1, (int)((b - a) * samplePerUnit));
        float step = (b - a) / nbSub;
        float stepT05 = step * 0.5f;//cache

        float a1, b1, I1, aPbO2;
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5f;//cache
            I1 = 0f;

            //integrate from a1 to b1 of f
            for (int j = 0; j < 5; j++)
            {
                I1 += wif[j] * f(stepT05 * xif[j] + aPbO2);
            }
            I += I1;
        }
        return stepT05 * I;
    }

    /// <summary>
    /// Rerturn the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static double Integrated(Func<double, double> f, in double a, in double b, in float samplePerUnit = 5f)
    {
        if (Math.Abs(a - b) < 1e-45d || samplePerUnit <= 0f)
            return 0d;
        if (a > b)
            return -Integrated(f, b, a, samplePerUnit);

        double I = 0d;

        int nbSub = Mathf.Max(1, (int)((b - a) * (double)samplePerUnit));
        double step = (b - a) / nbSub;
        double stepT05 = step * 0.5d;//cache

        double a1, b1, I1, aPbO2;
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5d;//cache
            I1 = 0d;

            //integrate from a1 to b1 of f
            for (int j = 0; j < 5; j++)
            {
                I1 += wid[j] * f(stepT05 * xid[j] + aPbO2);
            }
            I += I1;
        }
        return stepT05 * I;
    }

    /// <summary>
    /// Rerturn the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static decimal Integratem(Func<decimal, decimal> f, in decimal a, in decimal b, in float samplePerUnit = 5f)
    {
        if (Abs(a - b) < 1e-45m || samplePerUnit <= 0f)
            return 0m;
        if (a > b)
            return -Integratem(f, b, a, samplePerUnit);

        decimal I = 0m;

        int nbSub = Mathf.Max(1, (int)((float)(b - a) * samplePerUnit));
        decimal step = (b - a) / nbSub;
        decimal stepT05 = step * 0.5m;//cache

        decimal a1, b1, I1, aPbO2;
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5m;//cache
            I1 = 0m;

            //integrate from a1 to b1 of f
            for (int j = 0; j < 5; j++)
            {
                I1 += wim[j] * f(stepT05 * xim[j] + aPbO2);
            }
            I += I1;
        }
        return stepT05 * I;
    }

    private static float[] xif = new float[5] { 0f, 0.5384693f, -0.5384693f, 0.9061798f, -0.9061798f };
    private static double[] xid = new double[5] { 0d, 0.538469310105683d, -0.538469310105683d, 0.906179845938664d, -0.906179845938664d };
    private static decimal[] xim = new decimal[5] { 0m, 0.5384693101056830910363144206m, -0.5384693101056830910363144206m, 0.9061798459386639927976268782m, -0.9061798459386639927976268782m };
    private static float[] wif = new float[5] { 0.5688889f, 0.4786287f, 0.4786287f, 0.2369269f, 0.2369269f };
    private static double[] wid = new double[5] { 0.568888888888889d, 0.478628670499366d, 0.478628670499366d, 0.236926885056189d, 0.236926885056189d };
    private static decimal[] wim = new decimal[5] { 0.5688888888888888888888888889m, 0.4786286704993664680412915148m, 0.4786286704993664680412915148m, 0.2369268850561890875142640407m, 0.2369268850561890875142640407m };

    #endregion

    #region Opti

    private static Camera main;
    public static Camera mainCamera
    {
        get
        {
            if (main == null)
                main = Camera.main;
            return main;
        }
    }

    private static PointerEventData eventDataCurrentPos;
    private static List<RaycastResult> res;
    public static bool IsOverUI(in Vector3 position)
    {
        eventDataCurrentPos = new PointerEventData(EventSystem.current) { position = position };
        res = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPos, res);
        return res.Count > 0;
    }

    public static Vector2 GetWorldPositionCanvasElement(RectTransform elem)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(elem, elem.position, mainCamera, out var res);
        return res;
    }

    public static void DestroyChildren(this Transform t)
    {
        foreach (Transform child in t)
            UnityEngine.Object.Destroy(child.gameObject);
    }

    public static void DestroyChildren(this GameObject t)
    {
        foreach (Transform child in t.transform)
            UnityEngine.Object.Destroy(child.gameObject);
    }

    private static Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();
    public static WaitForSeconds GetWaitForSeconds(in float time)
    {
        if (waitForSecondsCache.TryGetValue(time, out WaitForSeconds waitForSeconds))
        {
            return waitForSeconds;
        }
        WaitForSeconds res = new WaitForSeconds(time);
        waitForSecondsCache.Add(time, res);
        return res;
    }

    #endregion

    #region Invoke

    private static BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreReturn |
           BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Default | BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy |
           BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.SetField |
           BindingFlags.SetProperty | BindingFlags.SuppressChangeType;

    /// <summary>
    /// Lance la fonction methodName de l'instance obj avec les paramètres param, T est le type de retour de la methode methodName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static T InvokeParams<T>(object obj, string methodName, object[] param) => (T)obj.GetType().GetMethod(methodName, flag).Invoke(obj, param);

    /// <summary>
    /// Lance la fonction void methodName de l'instance obj avec les paramètres param
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static void InvokeParams(object obj, string methodName, object[] param) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, param);

    public static void Invoke(this MonoBehaviour obj, string methodName, object[] param) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, param);
    public static void Invoke(this MonoBehaviour obj, string methodName, object[] param, float delay)
    {
        obj.StartCoroutine(InvokeCorout(obj, methodName, param, delay));
    }

    private static IEnumerator InvokeCorout(MonoBehaviour obj, string methodName, object[] param, float delay)
    {
        yield return GetWaitForSeconds(delay);
        Invoke(obj, methodName, param);
    }

    #region Invoke<T>

    public static void Invoke<T>(this MonoBehaviour obj, string methodName, T arg1) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, new object[1] { arg1 });

    public static void Invoke<T>(this MonoBehaviour obj, string methodName, T arg1, float delay)
    {
        obj.StartCoroutine(InvokeCorout(obj, methodName, arg1, delay));
    }

    private static IEnumerator InvokeCorout<T>(MonoBehaviour obj, string methodName,T arg1, float delay)
    {
        yield return GetWaitForSeconds(delay);
        Invoke(obj, methodName, arg1);
    }

    public static void Invoke<T1, T2>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, new object[2] { arg1, arg2 });
    public static void Invoke<T1, T2>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, float delay)
    {
        obj.StartCoroutine(InvokeCorout(obj, methodName, arg1, arg2, delay));
    }

    private static IEnumerator InvokeCorout<T1, T2>(MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, float delay)
    {
        yield return GetWaitForSeconds(delay);
        Invoke(obj, methodName, arg1, arg2);
    }

    public static void Invoke<T1, T2, T3>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, new object[3] { arg1, arg2, arg3 });
    public static void Invoke<T1, T2, T3>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3, float delay)
    {
        obj.StartCoroutine(InvokeCorout(obj, methodName, arg1, arg2, arg3, delay));
    }

    private static IEnumerator InvokeCorout<T1, T2, T3>(MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3, float delay)
    {
        yield return GetWaitForSeconds(delay);
        Invoke(obj, methodName, arg1, arg2, arg3);
    }

    public static void Invoke<T1, T2, T3, T4>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => obj.GetType().GetMethod(methodName, flag).Invoke(obj, new object[4] { arg1, arg2, arg3, arg4 });
    public static void Invoke<T1, T2, T3, T4>(this MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, float delay)
    {
        obj.StartCoroutine(InvokeCorout(obj, methodName, arg1, arg2, arg3, arg4, delay));
    }

    private static IEnumerator InvokeCorout<T1, T2, T3, T4>(MonoBehaviour obj, string methodName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, float delay)
    {
        yield return GetWaitForSeconds(delay);
        Invoke(obj, methodName, arg1, arg2, arg3, arg4);
    }

    #endregion

    #endregion

    #region Extension

    [Obsolete]
    public static T Clone<T>(this T obj)
    {
        string s = Save.ConvertObjectToJSONString(obj);
        if(s != "{}")
            return Save.ConvertJSONStringToObject<T>(s);

        MethodInfo inst = obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        return (T)inst?.Invoke(obj, null);
    }

    public static List<T> Clone<T>(this List<T> lst) => new List<T>(lst);

    [Obsolete]
    public static Texture2D ToTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }

    public static void AddToDontDestroyOnLoad(this GameObject obj)
    {
        obj.transform.parent = null;
        UnityEngine.Object.DontDestroyOnLoad(obj);
    }

    public static void RemoveFromDontDestroyOnLoad(this GameObject obj)
    {
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
    }

    public static void GizmoDrawVector(in Vector2 origin, in Vector2 dir) => GizmoDrawVector(origin, dir, 1f, 0.39269908169f); // 0.39269908169f = 22.5° = pi/8 rad
    public static void GizmoDrawVector(in Vector2 origin, in Vector2 dir, in float length) => GizmoDrawVector(origin, dir, length, 0.39269908169f);
    public static void GizmoDrawVector(in Vector2 origin, in Vector2 dir, in float length, in float arrowAngle)
    {
        Vector2 end = origin + dir * length;
        Gizmos.DrawLine(origin, end);
        float teta = AngleHori(origin, end);
        float a = Mathf.PI + teta + arrowAngle;
        Gizmos.DrawLine(end, end + new Vector2(length * 0.33f * Mathf.Cos(a), length * 0.33f * Mathf.Sin(a)));
        a = 2f * Mathf.PI - (Mathf.PI - teta) - arrowAngle;
        Gizmos.DrawLine(end, end + new Vector2(length * 0.33f * Mathf.Cos(a), length * 0.33f * Mathf.Sin(a)));
    }

    public static AnimationClip[] GetAnimationsClips(this Animator animator) => animator.runtimeAnimatorController.animationClips;

    public static string[] GetAnimations(this Animator animator)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        string[] res = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            res[i] = clips[i].name;
        }
        return res;
    }

    public static bool GetAnimationLength(this Animator anim, string name, out float length)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == name)
            {
                length = clips[i].length;
                return true;
            }
        }
        length = 0f;
        return false;
    }

    #endregion
}

#endregion