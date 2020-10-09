# ECS.Core Documentation


### I want to process one system at MonoBehaviour.Update() and another - at MonoBehaviour.FixedUpdate(). How I can do it?

```C#
void Start () {
    var world = new World ();
    var _update = 
        new Systems(world);
            .Add(updateSystem);
    update.Create();    

    _fixedUpdate = 
        new Systems(world)
            .Add (new FixedUpdateSystem());
    _fixedUpdate.Init ();
}

void Update () {
    _update.Run ();
}

void FixedUpdate () {
    _fixedUpdate.Run ();
}
```

