using System.Collections.Generic;
using UnityEngine;

class SymmetryEqualityComparer : IEqualityComparer<Symmetry>
{
    public bool Equals(Symmetry x, Symmetry y)
    {
        return x.matrixNormalizedByTranslation == y.matrixNormalizedByTranslation;
    }

    public int GetHashCode(Symmetry obj)
    {
        return obj.matrixNormalizedByTranslation.GetHashCode();
    }
}

public class Symmetry
{
    public Matrix4x4 myMatrix = Matrix4x4.zero;

    public Matrix4x4 matrixNormalizedByTranslation { get {
            Matrix4x4 m = Matrix4x4.zero;
            m[0, 0] = myMatrix[0, 0];
            m[0, 1] = myMatrix[0, 1];
            m[0, 2] = myMatrix[0, 2];
            m[1, 0] = myMatrix[1, 0];
            m[1, 1] = myMatrix[1, 1];
            m[1, 2] = myMatrix[1, 2];
            m[2, 0] = myMatrix[2, 0];
            m[2, 1] = myMatrix[2, 1];
            m[2, 2] = myMatrix[2, 2];
            m[0, 3] = myMatrix[0, 3] % 1;
            m[1, 3] = myMatrix[1, 3] % 1;
            m[2, 3] = myMatrix[2, 3] % 1;
            return m;
        }
    }

    public static Symmetry identity { get
        {
            Symmetry symm = new Symmetry
            {
                myMatrix = Matrix4x4.identity
            };
            symm.myMatrix[3, 3] = 0;
            return symm;
        }
    }

    public Symmetry() { }
    public Symmetry(Matrix4x4 matrix) { myMatrix = matrix; }

    public override string ToString()
    {
        string outp = "SYMM ";
        for(int i=0; i<3; i++)
        {
            Vector4 v = myMatrix.GetRow(i);
            string expr = "";
            if (v.x == 1) { expr += "+X"; } else if (v.x == -1) { expr += "-X"; }
            if (v.y == 1) { expr += "+Y"; } else if (v.y == -1) { expr += "-Y"; }
            if (v.z == 1) { expr += "+Z"; } else if (v.z == -1) { expr += "-Z"; }
            expr += (v.w>0?"+":"")+ v.w.ToString() + (i != 3 ? ", " : "");
            outp += expr;
        }
        return outp;
    }

    public Symmetry(string XExpr, string YExpr, string ZExpr)
    {
        XExpr = XExpr.Replace(" ", "").Replace("+", "");
        YExpr = YExpr.Replace(" ", "").Replace("+", "");
        ZExpr = ZExpr.Replace(" ", "").Replace("+", "");
        List<(string, int)> exprs = new List<(string, int)>
        {
            (XExpr, 0),
            (YExpr, 1),
            (ZExpr, 2)
        };
        foreach ((string, int) data in exprs)
        {
            int row = data.Item2;
            string expr = data.Item1;
            if (expr.Contains("X"))
            {
                if (expr.Contains("-X")) { myMatrix[row, 0] = -1; } else { myMatrix[row, 0] = 1; }
                expr = expr.Replace("-X", "").Replace("X", "");
            }
            if (expr.Contains("Y"))
            {
                if (expr.Contains("-Y")) { myMatrix[row, 1] = -1; } else { myMatrix[row, 1] = 1; }
                expr = expr.Replace("-Y", "Y").Replace("Y", "");
            }
            if (expr.Contains("Z"))
            {
                if (expr.Contains("-Z")) { myMatrix[row, 2] = -1; } else { myMatrix[row, 2] = 1; }
                expr = expr.Replace("-Z", "Z").Replace("Z", "");
            }
            if (expr.Length != 0)
            {
                Debug.Log(expr);
                myMatrix[row, 3] = float.Parse(expr);
            }
        }

        myMatrix.SetRow(3, new Vector4());
    }

    public Symmetry Compose(Symmetry other)
    {
        Matrix4x4 nmat = Matrix4x4.zero;
        Vector3 offsetVector = myMatrix.GetColumn(3);
        offsetVector = other.ApplyTransform(offsetVector);
        nmat.SetColumn(3, offsetVector);
        Matrix4x4 b = myMatrix;
        Matrix4x4 a = other.myMatrix;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for(int k=0; k<3; k++)
                {
                    nmat[i, j] += a[i, k] * b[k, j];
                }
            }
        }

        return new Symmetry(nmat);
    }

    public Vector3 ApplyTransform(Vector3 original)
    {
        Vector3 newVector = new Vector3();
        for (int row = 0; row < 3; row++) {
            Vector4 v = myMatrix.GetRow(row);
            newVector[row] = Vector3.Dot(original, new Vector3(v.x, v.y, v.z));
        }

        newVector += (Vector3)myMatrix.GetColumn(3);
        return newVector;
    }


    public static HashSet<Symmetry> ComposeAll(IEnumerable<Symmetry> original) {
        var ec = new SymmetryEqualityComparer();
        HashSet<Symmetry> symmetries = new HashSet<Symmetry>(original, ec);
        List<Symmetry> iterSymms;
        int prevlen = 0;
        while (symmetries.Count != prevlen)
        {
            prevlen = symmetries.Count;
            iterSymms = new List<Symmetry>(symmetries);
            foreach (Symmetry s in iterSymms)
            {
                foreach(Symmetry other in iterSymms)
                {
                    symmetries.Add(s.Compose(other));
                }
            }
        }

        return symmetries;

    }
}
