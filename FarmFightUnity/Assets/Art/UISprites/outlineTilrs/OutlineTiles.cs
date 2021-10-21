using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[Serializable]
public class OutlineTile 
{
    public int index;
    public int rotation;
    public Tile tile;
    


    public OutlineTile rotate(int number)
    {
        Debug.Assert(number < 6 && number >= 0);

        OutlineTile temp = new OutlineTile();

        temp.rotation += number;
        temp.rotation %= 6;

        temp.index = RotatedIndex(number);

        temp.tile = this.tile;

        return temp;
    }

    public int RotatedIndex(int number = 1)
    {
        return (index >> number | index << (6 - number)) & 63;
    }

    public void Draw(Tilemap tilemap, Hex position)
    {


        //the cell to draw to
        Vector3Int cell = (Vector3Int)position.Cell();

        //set the tile
        tilemap.SetTile(cell, tile);

        
        ///rotate the tile
        Matrix4x4 matrix = tilemap.GetTransformMatrix(cell);

        Quaternion rotationQ = Quaternion.Euler(0, 0, 60 * rotation);

        matrix.SetTRS(matrix.GetColumn(3), rotationQ, Vector3.one);

        tilemap.SetTransformMatrix(cell, matrix);
        
    }




}
