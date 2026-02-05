using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//implementação de ObjectPool usando System.Collections.Generic.Queue<T> para Unity.
public class ObjectPool 
{
    //O prefab a ser usado na Pool.
    private readonly GameObject prefab;

    //A Pool.
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    //Uma âncora de transform caso queira que os objetos da Pool sejam afiliados a um objeto especifico ou estejam presentes em um lugar especifico da cena.
    private readonly Transform anchor;

    //Tamanho padrão da Pool.
    private readonly int defaultSize;

    //Tamanho maximo da Pool.
    private readonly int maxSize;

    //Construtor da classe completo.
    public ObjectPool(GameObject prefab, int defaultSize, Transform anchor, int maxSize = int.MaxValue)
    {
        this.prefab = prefab;
        this.anchor = anchor;
        this.defaultSize = defaultSize;
        this.maxSize = maxSize;
        createPool();
    }
    //Construtor da classe sem a âncora.
    public ObjectPool(GameObject prefab, int defaultSize, int maxSize = int.MaxValue) : this(prefab, defaultSize, null, maxSize) { }
    
    //Inicializa a Pool.
    private void createPool()
    {
        for (int i = 0; i < defaultSize; i++)
        {
            GameObject instance = createPooledInstance();
            if (anchor != null)
            {
                instance.transform.SetParent(anchor.transform, true);
                instance.transform.SetLocalPositionAndRotation(Vector2.zero, Quaternion.identity);
            }
        }
    }
    
    //Cria uma instância na Pool.
    private GameObject createPooledInstance()
    {
        GameObject instance = GameObject.Instantiate(prefab);
        instance.SetActive(false);
        pool.Enqueue(instance);
        return instance;
    }

    //Pega um objeto da Pool, colocando-o numa posição fornecida, pode escolher se liga-o ou não.
    private GameObject getLogic(Vector2 position, Quaternion rotation, bool setActive, bool deAnchor)
    {
        GameObject instance = pool.Count > 0 ? pool.Dequeue() : createPooledInstance();
        if(setActive)
            instance.SetActive(true);
        if(anchor != null && deAnchor)
            instance.transform.SetParent(anchor, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }
    public GameObject get(Transform transform, bool setActive, bool deAnchor) => getLogic(transform.position, transform.rotation, setActive, deAnchor);

    public GameObject get(Transform transform, bool setActive) => getLogic(transform.position, transform.rotation, setActive, true);
    
    public GameObject get(Transform transform) => getLogic(transform.position, transform.rotation, true, true);

    public GameObject get(Vector2 position, bool setActive, bool deAnchor) => getLogic(position, Quaternion.identity, setActive, deAnchor);

    public GameObject get(Vector2 position, bool setActive) => getLogic(position, Quaternion.identity, setActive, true);

    public GameObject get(Vector2 position) => getLogic(position, Quaternion.identity, true, true);

    public GameObject get(Vector2 position, Quaternion rotation, bool setActive, bool deAnchor) => getLogic(position, rotation, setActive, deAnchor);

    public GameObject get(Vector2 position, Quaternion rotation, bool setActive) => getLogic(position, rotation, setActive, true);

    public GameObject get(Vector2 position, Quaternion rotation) => getLogic(position, rotation, true, true);

    //Pega uma quantidade especifica de objetos da Pool, colocando-os em uma posição fornecida, pode escolher se liga-os ou não.
    private IEnumerable<GameObject> getInBulkLogic(int amount, Vector2 position, Quaternion rotation, bool setActive, bool deAnchor)
    {
        GameObject[] instances = new GameObject[amount];
        for (int i = 0; i < amount; i++)
            instances[i] = getLogic(position, rotation, setActive, deAnchor);
        return instances;
    }
    
    private IEnumerable<GameObject> getInBulkIntoTransformsLogic(IEnumerable<Transform> tranformAndAmount, bool setActive, bool deAnchor)
    {
        List<GameObject> instances = new List<GameObject>(tranformAndAmount.Count());
        foreach(Transform t in tranformAndAmount)
            instances.Add(getLogic(t.position, t.rotation, setActive, deAnchor));
        return instances;
    }

    public GameObject[] getInBulkIntoTransformsAsArray(IEnumerable<Transform> tranformAndAmount, bool setActive) => getInBulkIntoTransformsLogic(tranformAndAmount, setActive, true).ToArray();

    public GameObject[] getInBulkIntoTransformsAsArray(IEnumerable<Transform> tranformAndAmount) => getInBulkIntoTransformsLogic(tranformAndAmount, true, true).ToArray();

    public List<GameObject> getInBulkIntoTransformsAsList(IEnumerable<Transform> tranformAndAmount, bool setActive) => getInBulkIntoTransformsLogic(tranformAndAmount, setActive, true).ToList<GameObject>();

    public List<GameObject> getInBulkIntoTransformsAsList(IEnumerable<Transform> tranformAndAmount) => getInBulkIntoTransformsLogic(tranformAndAmount, true, true).ToList<GameObject>();

    public GameObject[] getInBulkAsArray(int amount, Transform transform, bool setActive, bool deAnchor) => getInBulkLogic(amount, transform.position, transform.rotation, setActive, deAnchor).ToArray();

    public GameObject[] getInBulkAsArray(int amount, Transform transform, bool setActive) => getInBulkLogic(amount, transform.position, transform.rotation, setActive, true).ToArray();

    public GameObject[] getInBulkAsArray(int amount, Transform transform) => getInBulkLogic(amount, transform.position, transform.rotation, true, true).ToArray();

    public GameObject[] getInBulkAsArray(int amount, Vector2 position, bool setActive, bool deAnchor) => getInBulkLogic(amount, position, Quaternion.identity, setActive, deAnchor).ToArray();

    public GameObject[] getInBulkAsArray(int amount, Vector2 position, bool setActive) => getInBulkLogic(amount, position, Quaternion.identity, setActive, true).ToArray();

    public GameObject[] getInBulkAsArray(int amount, Vector2 position) => getInBulkLogic(amount, position, Quaternion.identity, true, true).ToArray();

    public List<GameObject> getInBulkAsList(int amount, Transform transform, bool setActive, bool deAnchor) => getInBulkLogic(amount, transform.position, transform.rotation, setActive, deAnchor).ToList();

    public List<GameObject> getInBulkAsList(int amount, Transform transform, bool setActive) => getInBulkLogic(amount, transform.position, transform.rotation, setActive, true).ToList();

    public List<GameObject> getInBulkAsList(int amount, Transform transform) => getInBulkLogic(amount, transform.position, transform.rotation, true, true).ToList();

    public List<GameObject> getInBulkAsList(int amount, Vector2 position, bool setActive, bool deAnchor) => getInBulkLogic(amount, position, Quaternion.identity, setActive, deAnchor).ToList();

    public List<GameObject> getInBulkAsList(int amount, Vector2 position, bool setActive) => getInBulkLogic(amount, position, Quaternion.identity, setActive, true).ToList();

    public List<GameObject> getInBulkAsList(int amount, Vector2 position) => getInBulkLogic(amount, position, Quaternion.identity, true, true).ToList();

    public void release(GameObject @object)
    {
        if (pool.Count >= maxSize)
        {
            GameObject.Destroy(@object);
            return;
        }
        @object.SetActive(false);
        if (anchor != null)
        {
            @object.transform.SetParent(anchor, true);
            @object.transform.SetLocalPositionAndRotation(Vector2.zero, Quaternion.identity);
        }
        pool.Enqueue(@object);
    }
    public void releaseAllIn(IEnumerable<GameObject> objects)
    {
        foreach (GameObject @object in objects)
            release(@object);
    }
}
