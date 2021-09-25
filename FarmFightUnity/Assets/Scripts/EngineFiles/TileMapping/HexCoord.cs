using System;
using UnityEngine;
public struct Hex
{
    public int x, y;

    public static Hex zero = new Hex(0, 0), up = new Hex(0, 1), down = new Hex(0, -1), right = new Hex(1, 0), left = new Hex(-1, 0);

    public Hex(int X, int Y)
    {
        this.x = X;
        this.y = Y;
    }


    public static Hex fromCell(int X, int Y)
    {
        int y = (Y - (X + (X & 1)) / 2);

        return new Hex(X, y);
    }

    public static Hex fromCell(Vector3Int cell)
    {


        return fromCell(cell.x, cell.y);
    }


    public static Vector2Int toCell(Hex coord)
    {

        int y = (coord.y + ((coord.x - (coord.x & 1)) / 2));

        return new Vector2Int(y,coord.x);
    }

    

    public Vector2Int Cell()
    {
        return toCell(this);
    }

    public Vector2 world(float yscale = 1)
    {
        Vector2Int cell = this.Cell();
        float R3 = Mathf.Sqrt(3);

        float Y = (int)((cell.y * R3 / (2*yscale)) - ((R3*yscale / 4) * (cell.x & 1)));

        return new Vector2((cell.x * .75f), Y);
    }

    public static Hex fromWorld(Vector3 coords, float yscale = 1)
    {
        float R3 = Mathf.Sqrt(3) / 2;

        int x = Mathf.RoundToInt(coords.x / 0.75f);
        int y = Mathf.RoundToInt((coords.y / (yscale*R3)) - ((x) * yscale * .5f));

        return new Hex(x, y);
    }
    
    static public Hex operator +(Hex self, Hex other)
    {
        return new Hex(self.x + other.x, self.y + other.y);
    }

    static public Hex operator +(Hex self, Vector2Int other)
    {
        return new Hex(self.x + other.x, self.y + other.y);
    }



    /// <summary>
    /// subtracting Hexcoords
    /// </summary>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    static public Hex operator -(Hex self, Hex other)
    {
        return new Hex(self.x - other.x, self.y - other.y);
    }

    /// <summary>
    /// adds the terms of the hexcoord
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    static public int operator +(Hex subject)
    {
        return subject.x + subject.y;
    }

    static public bool operator ==(Hex self, Hex other)
    {


        return (other.x == self.x) & (other.y == self.y);
    }
    static public bool operator !=(Hex self, Hex other)
    {
        return !(other == self);
    }


    /// <summary>
    /// negative overload
    /// </summary>
    /// <param name="sub"></param>
    /// <returns>negated hexcoord</returns>
    static public Hex operator -(Hex sub)
    {
        return new Hex(-sub.x, -sub.y);

    }


    static public Hex operator *(Hex self, int other)
    {
        return new Hex(other * self.x, other * self.y);
    }

    static public Hex operator /(Hex self, int other)
    {
        return new Hex(Mathf.RoundToInt(self.x / other), Mathf.RoundToInt(self.y / other));
    }


    public override string ToString()
    {
        return base.ToString() + ": " + (this.x, this.y).ToString();
    }

    public override bool Equals(object obj)
    {
        return this == (Hex)obj;
    }

    public override int GetHashCode()
    {
        return +this;
    }


}

