using System.Collections.Generic;

//les differents types de niveau, jai pris le jeu Hades comme exemple
public enum LevelType : byte
{
    enemyNormal,
    miniBoss,
    boss,
    marchaud,
    fontaine
}

// Contient toutes les info pour g�n�rer un niveau
public struct LevelData
{
    public int numeroEtage; //1, 2, 3 ou 4
    public LevelType levelType;

    public LevelData(int numeroEtage, LevelType levelType)
    {
        this.numeroEtage = numeroEtage;
        this.levelType = levelType;
    }
}

public struct GenerationData
{
    //mettre l'ensemble des param�tre de g�n�ration des niveau
    //Seul nombreDembranchement et profondeur est obligatoire
    public int nombreDembranchement;
    public int profondeur;
    public int nombreDeSalleAvantLaSalleDuBoss;
    public int nombreMaximalDeMiniBossParEtage;
}

//Une class accessible partout impl�mentant lalgo de g�n�ration al�atoire de niveau
// Cest la ou le gros du taff se fait, je suppose que tu as une fonction qui prend en entr�e tout les anciens niveau effectuer par le joueur
// et les eventuelles autre niveau fr�re (meme etage/profondeur dans l'arbre) et qui renvoie le nouveau LevelData a associ� au node.
public static class LevelGenerator
{
    public static LevelData GenerateNextLevelData(List<LevelNode> parents, List<LevelData> levelFreres, in GenerationData generationData)
    {
        // todo ; impl�ment� l'algo de gen�ration al�atoire
        // attention levelFreres peut etre vide
        return new LevelData(0, LevelType.enemyNormal);
    }

    //fonction qui cr�e le 1er niveau
    public static LevelData GenerateRootLevelData(in GenerationData generationData)
    {
        return new LevelData(0, LevelType.enemyNormal);
    }

    // La fonction a appel� avec un autre script pour lancer tout lalgo de g�n�ration et qui renvoie l'arbre g�n�r� al�atoirement
    public static LevelTree GenerateLevels(in GenerationData generationData)
    {
        //On cr�e le 1er niveau
        LevelNode root = new LevelNode(null, GenerateRootLevelData(generationData));

        List<LevelNode> parents = new List<LevelNode>(generationData.profondeur)
        {
            root
        };

        // on cr�e r�cursivement les autres
        GenerateChildrenRecur(parents, generationData, 1);

        return new LevelTree(root);
    }

    // Prend en entr� les parents d'un niveau et les parametre de g�n�ration et ajouter une profoncdeur de l'arbre par appel
    private static void GenerateChildrenRecur(List<LevelNode> parents, in GenerationData generationData, int depth)
    {
        if(depth > generationData.profondeur)
            return;

        List<LevelData> levelsFreres = new List<LevelData>(generationData.nombreDembranchement);

        //On cr�e les sous niveaux
        for (int i = 0; i < generationData.nombreDembranchement; i++)
        {
            LevelData levelData = GenerateNextLevelData(parents, levelsFreres, generationData);
            levelsFreres.Add(levelData);
        }

        // on ajoute les sopus niveau cr�e au dernier parent puis on continue r�cursivement
        LevelNode lastParent = parents[parents.Count - 1];
        depth++;
        foreach (LevelData levelData in levelsFreres)
        {
            LevelNode levelNode = new LevelNode(lastParent, levelData);
            lastParent.AddChild(levelNode);
            List<LevelNode> newParents = new List<LevelNode>(parents)
            {
                levelNode
            };
            GenerateChildrenRecur(newParents, generationData, depth);
        }
    }
}

public class LevelNode
{
    public LevelNode parent;
    public List<LevelNode> enfant;
    public LevelData levelData;

    public LevelNode(LevelNode parent, LevelData levelData)
    {
        this.parent = parent;
        enfant = new List<LevelNode>();
        this.levelData = levelData;
    }

    public void AddChild(LevelNode node)
    {
        enfant.Add(node);
    }
}

public class LevelTree
{
    public LevelNode root;

    public LevelTree(LevelNode root)
    {
        this.root = root;
    }
}