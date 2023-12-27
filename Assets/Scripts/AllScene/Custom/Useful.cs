#region using

using UnityEngine;
using Vec2 = UnityEngine.Vector2;
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
using DG.Tweening;
using System.Threading.Tasks;
using Collision2D;

#endregion

#region Unity special struct

[Serializable]
public struct ShakeSetting
{
    public float duration;
    public Vector3 strengh;
    public int vibrato;
    public float randomness;
    public bool snapping;
    public bool fadeOut;

    public ShakeSetting(float duration, Vector3 strengh, int vibrato, float randomness, bool snapping, bool fadeOut)
    {
        this.duration = duration;
        this.strengh = strengh;
        this.vibrato = vibrato;
        this.randomness = randomness;
        this.snapping = snapping;
        this.fadeOut = fadeOut;
    }
    public ShakeSetting(float duration = 1f, float strengh = 1f, int vibrato = 10, float randomness = 90f, bool snapping = false, bool fadeOut = true)
    {
        this.duration = duration;
        this.strengh = Vector3.one * strengh;
        this.vibrato = vibrato;
        this.randomness = randomness;
        this.snapping = snapping;
        this.fadeOut = fadeOut;
    }

    public void DefaultValue()
    {
        duration = 1f;
        strengh = Vector3.one;
        vibrato = 10;
        randomness = 90f;
        snapping = false;
        fadeOut = true;
    }

    public void ClampValue()
    {
        duration = Mathf.Max(0f, duration);
        strengh = new Vector3(Mathf.Max(0f, strengh.x), Mathf.Max(0f, strengh.y), Mathf.Max(0f, strengh.z));
        vibrato = vibrato >= 0 ? vibrato : 0;
        randomness = Mathf.Max(0f, randomness);
    }
}

#endregion

#region Save

public static class Save
{
    /// <summary>
    /// Convert any Serializable object in JSON string.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns> A string represent the object in parameter</returns>
    public static string ConvertObjectToJSONString(object obj, bool withIndentation = false) => JsonUtility.ToJson(obj, withIndentation);
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
    public static bool WriteJSONData(object objToWrite, string fileName, bool withIndentation = false)
    {
        try
        {
            string s = ConvertObjectToJSONString(objToWrite, withIndentation);
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
    /// Write in the customer machine a file with the object inside asynchronously
    /// </summary>
    /// <param name="objToWrite">The object to save</param>
    /// <param name="filename">the save path, begining to the game's folder</param>
    /// <param name="callback">The callback when the function end</param>
    /// <returns> true if the save complete successfully, false overwise</returns>
    public static async Task<bool> WriteJSONDataAsync(object objToWrite, string fileName, Action<bool> callback, bool withIndentation = false)
    {
        try
        {
            string s = ConvertObjectToJSONString(objToWrite, withIndentation);
            if (s == "{}")
            {
                callback.Invoke(false);
                return false;
            }
            await File.WriteAllTextAsync(Application.dataPath + fileName, s);
            callback.Invoke(true);
        }
        catch
        {
            callback.Invoke(false);
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
    public static void WriteJSONDataMultiThread(object objToWrite, string fileName, Action<bool> callback, bool withIndentation = false)
    {
        Thread thread = new Thread(func);
        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
        WriteMultiTreadData data = new WriteMultiTreadData(Application.dataPath + fileName, callback);
        thread.Start(data);

        void func(object rawData)
        {
            WriteMultiTreadData data = (WriteMultiTreadData)rawData;
            try
            {
                string s = ConvertObjectToJSONString(objToWrite, withIndentation);
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
    public static async Task<bool> ReadJSONDataAsync<T>(string fileName, Action<bool, T> callback)
    {
        try
        {
            string jsonString = await File.ReadAllTextAsync(Application.dataPath + fileName);
            if (jsonString == "{}")
            {
                callback.Invoke(false, default(T));
                return false;
            }
            callback.Invoke(true, ConvertJSONStringToObject<T>(jsonString));
            return true;
        }
        catch (Exception)
        {
            callback.Invoke(false, default(T));
            return false;
        }
    }

    /// <typeparam name="T">The object to read's type</typeparam>
    /// <param name="fileName">The path of the file, begining to the game's folder</param>
    /// <param name="objRead"></param>
    /// <returns> true if the function complete successfully, false overwise</returns>
    public static void ReadJSONDataMultiThread<T>(string fileName, Action<bool, T> callback)
    {
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

    public static async Task WriteStringAsync(string data, string fileName, Action<bool> callback, bool append = true)
    {
        await File.WriteAllTextAsync(Application.dataPath + fileName, data);
    }

    public static void WriteStringMultiThread(string data, string fileName, Action<bool> callback, bool append = true)
    {
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

    public static void ImageInPNGFormat(Color[] pixels, int w, int h, string path, string name, FilterMode filterMode = FilterMode.Point, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
    {
        Texture2D texture = GenerateImage(pixels, w, h, filterMode, textureWrapMode);
        File.WriteAllBytes(Application.dataPath + path + name + @".png", texture.EncodeToPNG());
    }

    public static void ImageInPNGFormat(Texture2D texture, string path, string name)
    {
        File.WriteAllBytes(Application.dataPath + path + name + @".png", texture.EncodeToPNG());
    }

    public static void ImageInJPGFormat(Color[] pixels, int w, int h, string path, string name, FilterMode filterMode = FilterMode.Point, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
    {
        Texture2D texture = GenerateImage(pixels, w, h, filterMode, textureWrapMode);
        File.WriteAllBytes(Application.dataPath + path + name + @".jpg", texture.EncodeToJPG());
    }

    public static void ImageInJPGFormat(Texture2D texture, string path, string name)
    {
        File.WriteAllBytes(Application.dataPath + path + name + @".jpg", texture.EncodeToJPG());
    }

    private static Texture2D GenerateImage(Color[] pixels, int w, int h, FilterMode filterMode = FilterMode.Point, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp)
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

    public static void SetSeed(int seed)
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
    public static int Rand(int a, int b) => random.Next(a, b + 1);
    /// <returns> A random float between 0 and 1, [0, 1]</returns>
    public static float Rand() => (float)random.Next(int.MaxValue) / (int.MaxValue - 1);
    /// <returns> A random float between a and b, [a, b]</returns>
    public static float Rand(float a, float b) => Rand() * Mathf.Abs(b - a) + a;
    /// <returns> A random int between a and b, [|a, b|[</returns>
    public static int RandExclude(int a, int b) => random.Next(a, b);
    /// <returns> A random double between a and b, [a, b[</returns>
    public static float RandExclude(float a, float b) => (float)random.NextDouble() * (Mathf.Abs(b - a)) + a;
    public static float RandExclude() => (float)random.NextDouble();
    public static float PerlinNoise(float x, float y) => Noise2d.Noise(x, y);
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
    public static Vector2 PointInCircle(in Vector2 center, float radius)
    {
        float angle = RandExclude(0f, twoPi);
        float length = Mathf.Sqrt(Rand()) * radius;
        return new Vector2(center.x + length * Mathf.Cos(angle), center.y + length * Mathf.Sin(angle));
    }
    public static Vector2 PointInRectangle(BoxCollider2D rec) => PointInRectangle(rec.transform.position, rec.size);
    public static Vector2 PointInRectangle(Hitbox rec) => PointInRectangle(rec.center, rec.size);
    public static Vector2 PointInRectangle(in Vector2 center, in Vector2 size)
    {
        return new Vector2(center.x + (Rand() - 0.5f) * size.x, center.y + (Rand() - 0.5f) * size.y);
    }

    public static Vector2 PointInCapsule(CapsuleCollider2D capsule) => PointInCapsule(new Capsule(capsule.transform.position, capsule.size, capsule.direction));
    public static Vector2 PointInCapsule(Capsule capsule)
    {
        float areaCircles = capsule.circle1.radius * capsule.circle1.radius * Mathf.PI;
        float areaRec = capsule.hitbox.size.y * capsule.hitbox.size.x;
        if(Rand(0f, areaCircles + areaRec) <= areaRec)
        {
            return PointInRectangle(capsule.hitbox.center, capsule.hitbox.size);
        }
        else
        {
            Vec2 v = capsule.circle1.center - capsule.hitbox.center;
            Vec2 rand = PointInCircle(Vec2.zero, capsule.circle1.radius);
            if(v.Dot(rand) >= 0f)
            {
                return capsule.circle1.center + rand;
            }
            return capsule.circle2.center + rand;
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
    public static int Bernoulli(float p) => Rand() <= p ? 1 : 0;
    public static int Binomial(int n, int p)
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
    public static float Normal(float esp, float sigma) => N01() * sigma + esp;

    #endregion

}

#endregion

#region 2DArray

[Serializable]
public class Array2D<T>
{
    public T[] array;
    public int width, height;

    public Array2D(int width, int height)
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
            res.Add(new List<T>());
            for (int j = 0; j < width; j++)
            {
                res[i].Add(array[j + i * width]);
            }
        }
        return res;
    }
}

#endregion

#region ICloneable<T>

public interface ICloneable<T>
{
    public T Clone();
}

#endregion

#region Polynome

public class Polynome
{
    public static Polynome Empty => new Polynome(new float[1]);

    public static Polynome Unit(int i)
    {
        float[] coeff = new float[i + 1];
        coeff[coeff.Length - 1] = 1f;
        return new Polynome(coeff);
    }

    public static float[] Degrees1Roots(Polynome p)
    {
        return new float[1] { -p.coefficient[0] / p.coefficient[1] };
    }

    public static float[] Degrees2Roots(Polynome p)
    {
        float a = p.coefficient[2];
        float b = p.coefficient[1];
        float c = p.coefficient[0];
        float delta = b * b - 4f * a * c;

        if (Mathf.Approximately(delta, 0f))
            return new float[1] { -b / (2f * a) };
        if (delta < 0f)
            return Array.Empty<float>();

        delta = Mathf.Sqrt(delta);
        return new float[2] { (-b - delta) / (2f * a), (-b + delta) / (2f * a) };
    }

    public static float[] Degrees3Roots(Polynome p)
    {
        double a = p.coefficient[3];
        double b = p.coefficient[2];
        double c = p.coefficient[1];
        double d = p.coefficient[0];

        float[] SolveCubicEquation(double a, double b, double c, double d)
        {
            // Cardano's formula
            double p = c / a - (b * b) / (3d * a * a);
            double q = (2d * (b * b * b) - 9d * a * b * c + 27d * (a * a) * d) / (27d * (a * a * a));

            double delta = (q * q) / 4d + (p * p * p) / 27d;

            if(delta.Approximately(0d))
            {
                return new float[1] { (float)(b / (-3d * a)) };
            }

            if (delta >= 0)
            {
                // One real root and two complex conjugate roots
                double u = -q / 2d + Math.Sqrt(delta);
                double v = -q / 2d - Math.Sqrt(delta);

                double realRoot = Math.Cbrt(u) - Math.Cbrt(v) - b / (3d * a);
                return new float[1] { (float)realRoot };
            }
            else // ok
            {
                double rho = Math.Sqrt((-p * p * p) / 27d);
                double theta = Math.Acos(-q / (2d * rho));

                rho = 2d * Math.Cbrt(rho);
                double offset = -b / (3d * a);

                double root1 = rho * Math.Cos(theta / 3d) + offset;
                double root2 = rho * Math.Cos((theta + 2d * Math.PI) / 3d) + offset;
                double root3 = rho * Math.Cos((theta + 4d * Math.PI) / 3d) + offset;

                return new float[3] { (float)root1, (float)root2, (float)root3 };
            }
        }

        return SolveCubicEquation(a, b, c, d);

        /*
        
        double P = -b * b / (3d * a * a) + c / a;
        double Q = (b / (27d * a)) * (2d * b * b / (a * a) - 9d * c / a) + d / a;

        // my own computation
        //P = ((3d * c * a) - (b * b)) / (3d * a * a);
        //Q = (-b / 3d) + (b * b * b / (9d * a * a)) + d - (b * c / (3d * a));

        float[] res = CardanFormula(P, Q);
        float tmp = (float)(b / (3d * a));
        for (int i = 0; i < res.Length; i++)
        {
            res[i] += tmp;
        }

        return res;


        //return the real roots of the function f(x) = x^3 + px + q
        float[] CardanFormula(double p, double q)
        {
            double delta = -(4d * p * p * p + 27d * q * q);
            
            if(((float)delta).Approximately(0f))
            {
                if (((float)p).Approximately(0f) && ((float)q).Approximately(0f))
                    return new float[1] { 0f };

                return new float[2]
                {
                    (float)(3d * q / p), (float)(-3d * q / (2d * p))
                };
            }

            if(delta > 0f)
            {
                double a = 2 * Math.Sqrt(-p / 3d);
                double b = Math.Acos((3d * q / (2d * p)) * Math.Sqrt(-3d / p)) / 3d;
                double c = 2d * Math.PI / 3d;
                return new float[3]
                {
                    (float)(a * Math.Cos(b)),
                    (float)(a * Math.Cos(b + c)),
                    (float)(a * Math.Cos(b + 2f * c))
                };
            }

            //delta < 0f
            double tmp = Math.Sqrt(-delta / 27d);
            double u = Math.Cbrt(0.5d * (tmp - q));
            double v = Math.Cbrt(-0.5d * (tmp + q));
            return new float[1]
            {
                (float)(u + v)
            };
        }
        */
    }

    public static float[] Degrees4Roots(Polynome p)
    {
        throw new NotImplementedException();
    }

    public float[] coefficient { get; private set; }
    private Polynome derivative;

    public int deg => coefficient.Length - 1;

    public Polynome(float[] coefficient, bool cacheDerivative = false)
    {
        if(coefficient.Length <= 0)
            coefficient = new float[1];
        this.coefficient = coefficient;
        RemoveUselessCoeff();

        if (cacheDerivative)
            derivative = Derivative();
    }

    private void RemoveUselessCoeff()
    {
        while (coefficient.Length > 1 && Mathf.Approximately(coefficient.Last(), 0f))
        {
            coefficient = coefficient.Where((float c, int i) => i < coefficient.Length - 1).ToArray();
        }
    }

    public Polynome Derivative()
    {
        float[] coeff = new float[Mathf.Max(coefficient.Length - 1, 0)];

        for (int i = 0; i < coeff.Length; i++)
        {
            coeff[i] = (i + 1) * coefficient[i + 1];
        }

        return new Polynome(coeff);
    }

    public float Evaluate(float x)
    {
        float result = coefficient.Last();

        for (int i = coefficient.Length - 2; i >= 0; i--)
            result = result * x + coefficient[i];

        return result;
    }

    public float Derivative(float x)
    {
        if(derivative != null)
            return derivative.Evaluate(x);
        return Derivative().Evaluate(x);
    }

    public float[] Roots() => Roots(1e-4f, 200);

    public float[] Roots(float accuracy, int maxIter)
    {
         if(deg == 0)
            return Array.Empty<float>();
        if (deg == 1)
            Degrees1Roots(this);
        if (deg == 2)
            return Degrees2Roots(this);
        if (deg == 3)
            return Degrees3Roots(this);
        //if (deg == 4)
        //    return Degrees4Roots(this);

        List<float> roots = new List<float>();
        RootsRecur(this, accuracy, maxIter, ref roots);

        return roots.ToArray();
    }

    private void RootsRecur(Polynome current, float accuracy, int maxIter, ref List<float> roots)
    {
        if(current.deg <= 3)
        {
            float[] rs = Array.Empty<float>();

            if (current.deg == 3)
                rs = Degrees3Roots(current);
            else if (current.deg == 2)
                rs = Degrees2Roots(current);
            else if (current.deg == 1)
                rs = Degrees1Roots(current);

            for (int i = 0; i < rs.Length; i++)
            {
                roots.Add(rs[i]);
            }
            return;
        }

        if(current.FindRoot(accuracy, maxIter, out float r))
        {
            roots.Add(r);
            (Polynome newP, Polynome _) = current / new Polynome(new float[2] { -r, 1f });
            RootsRecur(newP, accuracy, maxIter, ref roots);
        }
    }

    //Newtons method
    private bool FindRoot(float accuracy, int maxIter, out float root)
    {
        if (derivative == null)
            derivative = Derivative();

        root = Random.Rand(-20f, 20f);
        int iter = 0;
        float fx = Evaluate(root);

        do
        {
            root -= fx / derivative.Evaluate(root);
            fx = Evaluate(root);
            iter++;
        } while (Mathf.Abs(fx) > accuracy && iter < maxIter);

        return Mathf.Abs(fx) <= accuracy;
    }

    private (Polynome Q, Polynome R) EuclidianDivision(Polynome A, Polynome B)
    {
        Polynome Q = Polynome.Empty;
        Polynome R = A;

        float b = B.coefficient.Last();
        float a;
        while (R.deg >= B.deg)
        {
            a = R.coefficient.Last() / b;
            Q += Unit(R.deg - B.deg, a);
            R -= Unit(R.deg - B.deg, a) * B;
        }

        Polynome Unit(int i, float coeff)
        {
            float[] coeffs = new float[i + 1];
            coeffs[coeffs.Length - 1] = coeff;
            return new Polynome(coeffs);
        }

        return (Q, R);
    }

    public override bool Equals(object obj)
    {
        if(ReferenceEquals(this, null) && ReferenceEquals(obj, null))
            return true;
        if (ReferenceEquals(this, null) || ReferenceEquals(obj, null))
            return false;

        if(obj is  Polynome p)
        {
            return p.coefficient == coefficient;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(coefficient);
    }

    public override string ToString()
    {
        if(coefficient.Length <= 0)
            return "0";
        if (coefficient.Length == 1)
            return coefficient[0].ToString();
        if (coefficient.Length == 2)
            return coefficient[1] + "X + " + coefficient[0];

        StringBuilder sb = new StringBuilder();
        for (int i = coefficient.Length - 1; i > 1; i--)
        {
            sb.Append(coefficient[i]);
            sb.Append("X^");
            sb.Append(i);
            sb.Append(" + ");
        }

        sb.Append(coefficient[1]);
        sb.Append("X + ");
        sb.Append(coefficient[0]);
        return sb.ToString();
    }

    public static bool operator ==(Polynome p1, Polynome p2) => Equals(p1, p2);
    public static bool operator !=(Polynome p1, Polynome p2) => !(p1 == p2);
    public static Polynome operator -(Polynome p)
    {
        float[] coeff = new float[p.coefficient.Length];

        for (int i = 0; i < p.coefficient.Length; i++)
        {
            coeff[i] = -p.coefficient[i];
        }

        return new Polynome(coeff);
    }

    public static Polynome operator +(Polynome p1, Polynome p2)
    {
        float[] coeff = new float[Mathf.Max(p1.coefficient.Length, p2.coefficient.Length)];

        for (int i = 0; i < p1.coefficient.Length; i++)
        {
            coeff[i] = p1.coefficient[i];
        }
        for (int i = 0; i < p2.coefficient.Length; i++)
        {
            coeff[i] += p2.coefficient[i];
        }
        return new Polynome(coeff);
    }

    public static Polynome operator +(Polynome p1, float t)
    {
        float[] coeff = p1.coefficient;
        coeff[0] += t;
        return new Polynome(coeff);
    }

    public static Polynome operator +(float t, Polynome p1) => p1 + t;
    public static Polynome operator -(Polynome p1, float t) => p1 + (-t);
    public static Polynome operator -(float t, Polynome p1) => t + (-p1);

    public static Polynome operator -(Polynome p1, Polynome p2)
    {
        float[] coeff = new float[Mathf.Max(p1.coefficient.Length, p2.coefficient.Length)];

        for (int i = 0; i < p1.coefficient.Length; i++)
        {
            coeff[i] = p1.coefficient[i];
        }
        for (int i = 0; i < p2.coefficient.Length; i++)
        {
            coeff[i] -= p2.coefficient[i];
        }
        return new Polynome(coeff);
    }

    public static Polynome operator *(Polynome p1, Polynome p2)
    {
        float[] coeff = new float[p1.deg + p2.deg + 1];
        int m = p1.coefficient.Length - 1;
        int n = p2.coefficient.Length - 1;

        int end;
        for (int k = 0; k < coeff.Length; k++)
        {
            end = Mathf.Min(m, k);
            for (int i = Mathf.Max(0, k - n); i <= end; i++)
            {
                coeff[k] += p1.coefficient[i] * p2.coefficient[k - i];
            }
        }
        return new Polynome(coeff);
    }

    public static Polynome operator *(Polynome p1, float t)
    {
        float[] coeff = p1.coefficient;

        for (int i = 0; i < coeff.Length; i++)
        {
            coeff[i] *= t;
        }
        return new Polynome(coeff);
    }

    public static Polynome operator *(float t, Polynome p1) => p1 * t;


    public static (Polynome Q, Polynome R) operator /(Polynome p1, Polynome p2) => p1.EuclidianDivision(p1, p2);
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

    public static Color ColorRgbFromTemperature(float temperature)
    {
        // Temperature must fit between 1000 and 40000 degrees.
        temperature = Mathf.Clamp(temperature, 1000, 40000);

        // All calculations require temperature / 100, so only do the conversion once.
        temperature *= 0.01f;

        // Compute each color in turn.
        int red, green, blue;

        // First: red.
        if (temperature <= 66)
        {
            red = 255;
        }
        else
        {
            // Note: the R-squared value for this approximation is 0.988.
            red = (int)(329.698727446 * (Math.Pow(temperature - 60, -0.1332047592)));
            red = Mathf.Clamp(red, 0, 255);
        }

        // Second: green.
        if (temperature <= 66)
        {
            // Note: the R-squared value for this approximation is 0.996.
            green = (int)(99.4708025861 * Math.Log(temperature) - 161.1195681661);
        }
        else
        {
            // Note: the R-squared value for this approximation is 0.987.
            green = (int)(288.1221695283 * (Math.Pow(temperature - 60, -0.0755148492)));
        }

        green = Mathf.Clamp(green, 0, 255);

        // Third: blue.
        if (temperature >= 66)
        {
            blue = 255;
        }
        else if (temperature <= 19)
        {
            blue = 0;
        }
        else
        {
            // Note: the R-squared value for this approximation is 0.998.
            blue = (int)(138.5177312231 * Math.Log(temperature - 10) - 305.0447927307);
            blue = Mathf.Clamp(blue, 0, 255);
        }
        return new Color(red * 0.00392156862f, green * 0.00392156862f, blue * 0.00392156862f);//0.00392156862f = 1f/255f
    }

    public static Texture2D Lerp(Texture2D A, Texture2D B, float t)
    {
        if (A.width != B.width || A.height != B.height)
        {
            int w = Mathf.Min(A.width, B.width);
            int h = Mathf.Min(A.height, B.height);
            if (A.width < B.width || A.height < B.height)
                A.Reinitialize(w, h);
            if (B.width < A.width || B.height < A.height)
                B.Reinitialize(w, h);
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
    public static Vector2 Vector2FromAngle(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    public static Vector2 Vector2FromAngle(float angle, float length) => new Vector2(length * Mathf.Cos(angle), length * Mathf.Sin(angle));


    public static Vector3 ToVector3(in this Vector2 v) => new Vector3(v.x, v.y);
    public static Vector4 ToVector4(in this Vector2 v) => new Vector4(v.x, v.y);
    /// <returns>the orthogonal normalised vector of v</returns>
    public static Vector2 NormalVector(in this Vector2 v)
    {
        if (!Mathf.Approximately(v.x, 0f))
        {
            float y = Mathf.Sqrt(1f / (((v.y * v.y) / (v.x * v.x)) + 1f));
            return new Vector2(-v.y * y / v.x, y);
        }
        else if (!Mathf.Approximately(v.y, 0f))
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
    public static float Sign(this float a) => Mathf.Approximately(a, 0f) ? 0f : (a > 0f ? 1f : -1f);
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
    public static float ClampModulo(float start, float end, float value)
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

    /// <returns> a like a = value % (end -  start) + start, a€[start, end[ /returns>
    public static int ClampModulo(int start, int end, int value)
    {
        if (end < start)
            return ClampModulo(end, start, value);
        if (end == start)
            return start;

        if (value < end && value >= start)
            return value;
        else
        {
            int modulo = end - start;
            int result = ((value - start) % modulo) + start;
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
    public static void DirectionAngle(float ang1, float ang2, out bool right)
    {
        float diff = Mathf.Abs(ang1 - ang2);
        float angMin = Mathf.Min(diff, 2f * Mathf.PI - diff);
        right = Mathf.Abs((ang1 + angMin) % (2f * Mathf.PI) - ang2) < 0.1f;
    }
    /// <summary>
    /// Renvoie la valeur arrondi de n
    /// </summary>
    public static int Round(this float n) => (n - Mathf.Floor(n)) >= 0.5f ? (int)n + 1 : (int)n;
    /// <summary>
    /// Renvoie la valeur arrondi de n au nombre de décimales en param, ex : Round(51.6854, 2) => 51.69
    /// </summary>
    public static float Round(this float n, int nbDecimals)
    {
        float npow = n * Mathf.Pow(10f, nbDecimals);
        return npow - (int)npow >= 0.5f ? (((int)(npow + 1)) / Mathf.Pow(10f, nbDecimals)) : (((int)npow) / Mathf.Pow(10f, nbDecimals));
    }
    public static int Round(this double n) => (int)Math.Round(n);
    public static int Round(this double n, int nbDecimals) => (int)Math.Round(n, nbDecimals);
    public static int Floor(this float n) => Mathf.FloorToInt(n);
    public static int Ceil(this float n) => Mathf.CeilToInt(n);
    public static int Floor(this double n) => (int)Math.Floor(n);
    public static int Ceil(this double n) => (int)Math.Ceiling(n);

    public static bool Approximately(this float a, float b)
    {
        return MathF.Abs(b - a) < 1e-5f * MathF.Max(MathF.Pow(10f, MathF.Ceiling(MathF.Log10(MathF.Max(a, b)))), 1f);
    }

    public static bool Approximately(this double a, double b)
    {
        return Math.Abs(b - a) < 1e-11d * Math.Max(Math.Pow(10d, Math.Ceiling(Math.Log10(Math.Max(a, b)))), 1d);
    }

    public static int Max(int a, int b) => a >= b  ? a : b;
    public static int Max(int a, int b, int c) => Max(c, Max(a, b));
    public static int Max(params int[] args)
    {
        int max = args[0];
        for (int i = 1; i < args.Length; i++)
        {
            max = Max(max, args[i]);
        }
        return max;
    }
    public static int Min(int a, int b) => a <= b ? a : b;
    public static int Min(int a, int b, int c) => Min(c, Min(a, b));
    public static int Min(params int[] args)
    {
        int min = args[0];
        for (int i = 1; i < args.Length; i++)
        {
            min = Min(min, args[i]);
        }
        return min;
    }

    /// <summary>
    /// t € [0, 1]
    /// </summary>
    public static int Lerp(in int a, in int b, float t) => (int)(a + (b - a) * t);
    public static float Lerp(in float a, in float b, float t) => a + (b - a) * t;

    public static bool IsOdd(this int number) => (number & 1) != 0;
    public static bool IsEven(this int number) => (number & 1) == 0;

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

    public enum Side { Up, Down, Right, Left }

    public static bool IsPointInRectangle(in Vector2 center, in Vector2 size, in Vector2 point)
    {
        return point.x >= center.x - size.x * 0.5f && point.x <= center.x + size.x * 0.5f && point.y >= center.y - size.y * 0.5f && point.y <= center.y + size.y * 0.5f;
    }

    public static Side GetRectangleSide(in Vector2 center, in Vector2 size, in Vector2 point)
    {
        bool up = IsPointInRectangle(new Vector2(center.x, center.y + (size.y * 0.25f)), new Vector2(size.x, size.y * 0.5f), point);
        bool down = !up && IsPointInRectangle(new Vector2(center.x, center.y - (size.y * 0.25f)), new Vector2(size.x, size.y * 0.5f), point);

        if(up || down)
        {
            bool right = IsPointInRectangle(new Vector2(center.x + (size.x * 0.25f), center.y), new Vector2(size.x * 0.5f, size.y), point);
            float vPercent = Mathf.Abs(point.y - center.y) / size.y;
            float hPercent = Mathf.Abs(point.x - center.x) / size.x;
            return vPercent >= hPercent ? (up ? Side.Up : Side.Down) : (right ? Side.Right : Side.Left);
        }

        float angle1, angle2;
        Vector2 topRight = new Vector2(center.x + (size.x * 0.5f), center.y + (size.y * 0.5f));
        Vector2 topLeft = new Vector2(center.x - (size.x * 0.5f), center.y + (size.y * 0.5f));

        angle1 = AngleHori(topRight, point);
        angle2 = AngleHori(topLeft, point);
        if (angle1 >= Mathf.PI * 0.25f && angle2 <= Mathf.PI * 0.75f)
        {
            return Side.Up;
        }

        Vector2 botRight = new Vector2(center.x + (size.x * 0.5f), center.y - (size.y * 0.5f));

        angle2 = AngleHori(botRight, point);
        if ((angle1 <= Mathf.PI * 0.25f || angle1 >= 1.5f * Mathf.PI) && (angle2 <= Mathf.PI * 0.5f || angle2 >= Mathf.PI * 1.75f))
        {
            return Side.Right;
        }

        Vector2 botLeft = new Vector2(center.x - (size.x * 0.5f), center.y - (size.y * 0.5f));
        angle1 = AngleHori(botLeft, point);
        if (angle1 >= Mathf.PI * 1.25f && angle2 <= Mathf.PI * 1.75f)
        {
            return Side.Down;
        }

        angle2 = AngleHori(topLeft, point);
        if ((angle1 <= Mathf.PI * 1.25f && angle1 >= Mathf.PI * 0.5f) && angle2 >= Mathf.PI * 0.75f)
        {
            return Side.Left;
        }

        return Side.Up;
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

    public static T[] Clone<T>(this T[] array) where T : ICloneable<T>
    {
        T[] res = new T[array.Length];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = array[i].Clone();
        }
        return res;
    }

    public static T[,] Clone<T>(this T[,] Array)
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

    public static T[] Merge<T>(this T[] arr, T[] other)
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

    public static void Print<T>(this T[] tab, string begMessage = "", string endMessage = "")
    {
        Debug.Log(begMessage + tab.ToString<T>() + endMessage);
    }

    public static string ToString<T>(this T[] arr)
    {
        if (arr.Length <= 0)
            return "[]";

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
        for (int i = list.Count - 1; i >= 0; i--)
        {
            int j = Random.Rand(0, i);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static void Shuffle<T>(this T[] list)
    {
        for (int i = list.Length - 1; i >= 0; i--)
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
    public static void ShufflePartialy<T>(this List<T> list, float percentage)
    {
        int nbPermut = Max((int)(list.Count * percentage), 1);
        for (int i = 0; i < nbPermut; i++)
        {
            int randIndex1 = Random.RandExclude(0, list.Count);
            int randIndex2 = Random.RandExclude(0, list.Count);
            T temp = list[randIndex1];
            list[randIndex1] = list[randIndex2];
            list[randIndex2] = temp;
        }
    }

    /// <summary>
    /// Shuffle a little bit the list, reproduce approximately the real life
    /// </summary>
    /// <param name="percentage">The percentage to shuffle between 0 and 1</param>
    public static void ShufflePartialy<T>(this T[] list, float percentage)
    {
        int nbPermut = Max((int)(list.Length * percentage), 1);
        for (int i = 0; i < nbPermut; i++)
        {
            int randIndex1 = Random.RandExclude(0, list.Length);
            int randIndex2 = Random.RandExclude(0, list.Length);
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

    public static int ToInt(this string number)
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

    public static float ToFloat(this string number)
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
        result = ToInt(partieEntiere);
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

    public static List<T> Distinct<T>(this List<T> lst)
    {
        List<T> result = new List<T>();
        foreach (T item in lst)
        {
            if(!result.Contains(item))
                result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Retourne lst1 union lst2
    /// </summary>
    /// <param name="lst1">La première liste</param>
    /// <param name="lst2">La seconde liste</param>
    /// <param name="doublon">SI on autorise ou pas les doublons</param>
    /// <returns></returns>     
    public static List<T> Merge<T>(this List<T> lst1, in List<T> lst2, bool doublon = false)//pas de doublon par defaut
    {
        return Merge(lst1, lst2);
    }

    public static List<T> Merge<T>(in List<T> lst1, in List<T> lst2, bool doublon = false)//pas de doublon par defaut
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

    public static List<T> DeepClone<T>(this List<T> lst) where T : ICloneable<T>
    {
        List<T> res = new List<T>();
        foreach (T item in lst)
        {
            res.Add(item.Clone());
        }
        return res;
    }

    #endregion

    #region Integrate

    private static readonly float[] xf = { -0.99312859918f, -0.96397192727f, -0.91223442825f, -0.83911697182f, -0.74633190646f, -0.63605368072f, -0.51086700195f, -0.37370608871f, -0.22778585114f, -0.07652652113f };
    private static readonly float[] wf = { 0.0176140071f, 0.0406014298f, 0.0626720483f, 0.0832767415f, 0.1019301198f, 0.1181945319f, 0.1316886384f, 0.1420961093f, 0.1491729864f, 0.1527533871f };
    private static readonly double[] xd = { -0.99312859918509492479, -0.96397192727791379127, -0.91223442825132590587, -0.83911697182221882339, -0.74633190646015079261, -0.63605368072651502545, -0.51086700195082709800, -0.37370608871541956067, -0.22778585114164507808, -0.07652652113349733375 };
    private static readonly double[] wd = { 0.01761400713915211831, 0.04060142980038694133, 0.06267204833410906357, 0.08327674157670474872, 0.10193011981724043504, 0.11819453196151841731, 0.13168863844917662690, 0.14209610931838205133, 0.14917298647260374679, 0.15275338713072585070, };
    private static readonly decimal[] xm = {-0.99312859918509492478612238847132027822264464433825m, -0.96397192727791379126766613119727722191206032780642m, -0.91223442825132590586775244120329811304998996651304m, -0.83911697182221882339452906170152068532962931231767m, -0.74633190646015079261430507035564159031073067668055m, -0.63605368072651502545283669622628593674338911647884m, -0.51086700195082709800436405095525099842543987456776m, -0.37370608871541956067254817702492723739574618234639m, -0.22778585114164507808049619536857462474308816338187m, -0.07652652113349733375464040939883821100467147181940m };
    private static readonly decimal[] wm = { 0.01761400713915211831186196235185281636214355980522m, 0.04060142980038694133103995227493210987909063994881m, 0.06267204833410906356950653518704160635160186567984m, 0.08327674157670474872475814322204620610017782858333m, 0.10193011981724043503675013548034987616669109253665m, 0.11819453196151841731237737771138228700504121954834m, 0.13168863844917662689849449974816313491611051175818m, 0.14209610931838205132929832506716493303451541339205m, 0.14917298647260374678782873700196943669267926962838m, 0.15275338713072585069808433195509759349194896951277m };

    /// <summary>
    /// Return the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static float Integrate(Func<float, float> f, float a, float b, float samplePerUnit = 1f)
    {
        if (Mathf.Approximately(a, b) || samplePerUnit < 0f)
            return 0f;
        if (a > b)
            return -Integrate(f, b, a, samplePerUnit);

        float I = 0f;

        int nbSub = Mathf.Max(1, (int)((b - a) * samplePerUnit));
        float step = (b - a) / nbSub;
        float stepT05 = step * 0.5f;//cache

        float a1, b1, aPbO2;
        float[] cache = new float[10];
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5f;//cache

            cache[0] = stepT05 * xf[0];
            cache[1] = stepT05 * xf[1];
            cache[2] = stepT05 * xf[2];
            cache[3] = stepT05 * xf[3];
            cache[4] = stepT05 * xf[4];
            cache[5] = stepT05 * xf[5];
            cache[6] = stepT05 * xf[6];
            cache[7] = stepT05 * xf[7];
            cache[8] = stepT05 * xf[8];
            cache[9] = stepT05 * xf[9];

            I += wf[0] * (f(cache[0] + aPbO2) + f(-cache[0] + aPbO2)) +
                wf[1] * (f(cache[1] + aPbO2) + f(-cache[1] + aPbO2)) +
                wf[2] * (f(cache[2] + aPbO2) + f(-cache[2] + aPbO2)) +
                wf[3] * (f(cache[3] + aPbO2) + f(-cache[3] + aPbO2)) +
                wf[4] * (f(cache[4] + aPbO2) + f(-cache[4] + aPbO2)) +
                wf[5] * (f(cache[5] + aPbO2) + f(-cache[5] + aPbO2)) +
                wf[6] * (f(cache[6] + aPbO2) + f(-cache[6] + aPbO2)) +
                wf[7] * (f(cache[7] + aPbO2) + f(-cache[7] + aPbO2)) +
                wf[8] * (f(cache[8] + aPbO2) + f(-cache[8] + aPbO2)) +
                wf[9] * (f(cache[9] + aPbO2) + f(-cache[9] + aPbO2));
        }

        return stepT05 * I;
    }

    /// <summary>
    /// Return the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static double Integrate(Func<double, double> f, in double a, in double b, in float samplePerUnit = 1f)
    {
        if (Math.Abs(a - b) < 1e-10d || samplePerUnit <= 0f)
            return 0d;
        if (a > b)
            return -Integrate(f, b, a, samplePerUnit);

        double I = 0d;

        int nbSub = Mathf.Max(1, (int)((b - a) * (double)samplePerUnit));
        double step = (b - a) / nbSub;
        double stepT05 = step * 0.5d;//cache

        double a1, b1, aPbO2;
        double[] cache = new double[20];
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5d;//cache

            cache[0] = stepT05 * xd[0];
            cache[1] = stepT05 * xd[1];
            cache[2] = stepT05 * xd[2];
            cache[3] = stepT05 * xd[3];
            cache[4] = stepT05 * xd[4];
            cache[5] = stepT05 * xd[5];
            cache[6] = stepT05 * xd[6];
            cache[7] = stepT05 * xd[7];
            cache[8] = stepT05 * xd[8];
            cache[9] = stepT05 * xd[9];
            cache[10] = -cache[0];
            cache[11] = -cache[1];
            cache[12] = -cache[2];
            cache[13] = -cache[3];
            cache[14] = -cache[4];
            cache[15] = -cache[5];
            cache[16] = -cache[6];
            cache[17] = -cache[7];
            cache[18] = -cache[8];
            cache[19] = -cache[9];

            I += wd[0] * (f(cache[0] + aPbO2) + f(cache[10] + aPbO2)) +
                wd[1] * (f(cache[1] + aPbO2) + f(cache[11] + aPbO2)) +
                wd[2] * (f(cache[2] + aPbO2) + f(cache[12] + aPbO2)) +
                wd[3] * (f(cache[3] + aPbO2) + f(cache[13] + aPbO2)) +
                wd[4] * (f(cache[4] + aPbO2) + f(cache[14] + aPbO2)) +
                wd[5] * (f(cache[5] + aPbO2) + f(cache[15] + aPbO2)) +
                wd[6] * (f(cache[6] + aPbO2) + f(cache[16] + aPbO2)) +
                wd[7] * (f(cache[7] + aPbO2) + f(cache[17] + aPbO2)) +
                wd[8] * (f(cache[8] + aPbO2) + f(cache[18] + aPbO2)) +
                wd[9] * (f(cache[9] + aPbO2) + f(cache[19] + aPbO2));
        }
        return stepT05 * I;
    }

    /// <summary>
    /// Return the integral between a and b of f(x)dx
    /// </summary>
    /// <param name="function">La function à intégré</param>
    /// <param name="a">le début de l'intégrale</param>
    /// <param name="b">la fin de l'intégrale</param>
    /// <param name="stepPerUnit">le nombre de subdivision par unité d'intégration <=> la précision</param>
    /// <returns>The integral between a and b of f(x)dx</returns>
    public static decimal Integrate(Func<decimal, decimal> f, in decimal a, in decimal b, in float samplePerUnit = 1f)
    {
        if (Abs(a - b) < 1e-27m || samplePerUnit <= 0f)
            return 0m;
        if (a > b)
            return -Integrate(f, b, a, samplePerUnit);

        decimal I = 0m;

        int nbSub = Mathf.Max(1, (int)((float)(b - a) * samplePerUnit));
        decimal step = (b - a) / nbSub;
        decimal stepT05 = step * 0.5m;//cache

        decimal a1, b1, aPbO2;
        decimal[] cache = new decimal[20];
        for (int i = 0; i < nbSub; i++)
        {
            a1 = a + i * step;
            b1 = a1 + step;

            aPbO2 = (a1 + b1) * 0.5m;//cache

            cache[0] = stepT05 * xm[0];
            cache[1] = stepT05 * xm[1];
            cache[2] = stepT05 * xm[2];
            cache[3] = stepT05 * xm[3];
            cache[4] = stepT05 * xm[4];
            cache[5] = stepT05 * xm[5];
            cache[6] = stepT05 * xm[6];
            cache[7] = stepT05 * xm[7];
            cache[8] = stepT05 * xm[8];
            cache[9] = stepT05 * xm[9];
            cache[10] = -cache[0];
            cache[11] = -cache[1];
            cache[12] = -cache[2];
            cache[13] = -cache[3];
            cache[14] = -cache[4];
            cache[15] = -cache[5];
            cache[16] = -cache[6];
            cache[17] = -cache[7];
            cache[18] = -cache[8];
            cache[19] = -cache[9];

            I += wm[0] * (f(cache[0] + aPbO2) + f(cache[10] + aPbO2)) +
                wm[1] * (f(cache[1] + aPbO2) + f(cache[11] + aPbO2)) +
                wm[2] * (f(cache[2] + aPbO2) + f(cache[12] + aPbO2)) +
                wm[3] * (f(cache[3] + aPbO2) + f(cache[13] + aPbO2)) +
                wm[4] * (f(cache[4] + aPbO2) + f(cache[14] + aPbO2)) +
                wm[5] * (f(cache[5] + aPbO2) + f(cache[15] + aPbO2)) +
                wm[6] * (f(cache[6] + aPbO2) + f(cache[16] + aPbO2)) +
                wm[7] * (f(cache[7] + aPbO2) + f(cache[17] + aPbO2)) +
                wm[8] * (f(cache[8] + aPbO2) + f(cache[18] + aPbO2)) +
                wm[9] * (f(cache[9] + aPbO2) + f(cache[19] + aPbO2));
        }
        return stepT05 * I;
    }

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
    public static WaitForSeconds GetWaitForSeconds(float time)
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

    #region Invoke simple

    public static void InvokeWaitAFrame(this MonoBehaviour script, string methodName)
    {
        script.StartCoroutine(InvokeWaitAFrameCorout(script, methodName));
    }

    private static IEnumerator InvokeWaitAFrameCorout(MonoBehaviour script, string methodName)
    {
        yield return null;
        script.Invoke(methodName, 0f);
    }

    public static void Invoke(this MonoBehaviour script, Action method, float delay)
    {
        script.StartCoroutine(InvokeCorout(method, delay));
    }

    private static IEnumerator InvokeCorout(Action method, float delay)
    {
        yield return GetWaitForSeconds(delay);
        method.Invoke();
    }

    #endregion

    #region Invoke<T>

    public static void Invoke<T>(this MonoBehaviour script, Action<T> method, T param, float delay)
    {
        script.StartCoroutine(InvokeCorout(method, param, delay));
    }

    private static IEnumerator InvokeCorout<T>(Action<T> method, T param, float delay)
    {
        yield return GetWaitForSeconds(delay);
        method.Invoke(param);
    }

    public static void Invoke<T1, T2>(this MonoBehaviour script, Action<T1, T2> method, T1 param1, T2 param2, float delay)
    {
        script.StartCoroutine(InvokeCorout(method, param1, param2, delay));
    }

    private static IEnumerator InvokeCorout<T1, T2>(Action<T1, T2> method, T1 param1, T2 param2, float delay)
    {
        yield return GetWaitForSeconds(delay);
        method.Invoke(param1, param2);
    }

    public static void Invoke<T1, T2, T3>(this MonoBehaviour script, Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3, float delay)
    {
        script.StartCoroutine(InvokeCorout(method, param1, param2, param3, delay));
    }

    private static IEnumerator InvokeCorout<T1, T2, T3>(Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3, float delay)
    {
        yield return GetWaitForSeconds(delay);
        method.Invoke(param1, param2, param3);
    }

    #endregion

    #region InvokeRepeating

    public static void InvokeRepeating(this MonoBehaviour script, Action method, float deltaTime)
    {
        script.StartCoroutine(InvokeRepeatingCorout(method, deltaTime, -1f));
    }

    public static void InvokeRepeating(this MonoBehaviour script, Action method, float deltaTime, float duration)
    {
        script.StartCoroutine(InvokeRepeatingCorout(method, deltaTime, duration));
    }

    private static IEnumerator InvokeRepeatingCorout(Action method, float deltaTime, float duration)
    {
        float timeBeg = Time.time;
        float time = Time.time;

        while(duration < Mathf.Epsilon || Time.time - timeBeg < duration)
        {
            if(Time.time - time < deltaTime)
            {
                method.Invoke();
                time = Time.time;
            }
            yield return null;
        }
    }

    #endregion

    #region InvokeRepeating<T>

    public static void InvokeRepeating<T>(this MonoBehaviour script, Action<T> method, T param, float deltaTime)
    {
        script.StartCoroutine(InvokeRepeatingCorout(method, param, deltaTime, -1f));
    }

    public static void InvokeRepeating<T>(this MonoBehaviour script, Action<T> method, T param, float deltaTime, float duration)
    {
        script.StartCoroutine(InvokeRepeatingCorout(method, param, deltaTime, duration));
    }

    private static IEnumerator InvokeRepeatingCorout<T>(Action<T> method, T param, float deltaTime, float duration)
    {
        float timeBeg = Time.time;
        float time = Time.time;

        while (duration < Mathf.Epsilon || Time.time - timeBeg < duration)
        {
            if (Time.time - time < deltaTime)
            {
                method.Invoke(param);
                time = Time.time;
            }
            yield return null;
        }
    }

    #endregion

    #endregion

    #region Unity

    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dict) => new Dictionary<TKey, TValue>(dict);
    public static Dictionary<TKey, TValue> DeepClone<TKey, TValue>(this Dictionary<TKey, TValue> dict) where TValue : ICloneable<TValue>
    {
        Dictionary<TKey, TValue> res = new Dictionary<TKey, TValue>();
        foreach(KeyValuePair<TKey, TValue> kvp in dict)
        {
            res.Add(kvp.Key, kvp.Value.Clone());
        }
        return res;
    }

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

    public static void AddToDontDestroyOnLoad(this MonoBehaviour obj)
    {
        obj.transform.parent = null;
        UnityEngine.Object.DontDestroyOnLoad(obj.gameObject);
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

    public static void Shake(this Camera cam, in ShakeSetting cameraShakeSetting)
    {
        Shake(cam.transform, cameraShakeSetting);
    }

    public static void Shake(this Transform t, in ShakeSetting shakeSetting)
    {
        t.DOShakePosition(shakeSetting.duration, shakeSetting.strengh, shakeSetting.vibrato, shakeSetting.randomness,
            shakeSetting.snapping, shakeSetting.fadeOut);
    }

    #endregion
}

#endregion