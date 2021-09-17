using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBehavior : TileBehavior
{
    public BasicBehavior()
    {
        art = ArtRepository.Art["Test"];
    }
}

public class SelectBehavior : TileBehavior
{
    public SelectBehavior()
    {
        art = ArtRepository.Art["Select"];
    }
}