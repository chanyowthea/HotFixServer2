using UnityEngine;

namespace GCommon
{
    public class MathUnity
    {
        public const float EPSILON = 0.00001f;
        public const float RAD0 = 0f;
        public const float RADHALF = 0.008726646f;
        public const float RAD1 = 0.017453293f;
        public const float RAD5 = 0.087266463f;
        public const float RAD10 = 0.174532925f;
        public const float RAD15 = 0.261799388f;
        public const float RAD22HALF = 0.392699082f;
        public const float RAD30 = 0.523598776f;
        public const float RAD45 = 0.785398163f;
        public const float RAD60 = 1.047197551f;
        public const float RAD90 = 1.570796327f;
        public const float RAD120 = 2.094395102f;
        public const float RAD135 = 2.35619449f;
        public const float RAD180 = 3.141592654f;

        public static Vector3 Vector3ZeroY(Vector3 v)
        {
            Vector3 v2d = v;
            v2d.y = 0;
            return v2d;
        }
        public static Vector3 VectorWithY(Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }
        public static Vector3 GetDirection2D(Vector3 to, Vector3 from)
        {
            to.y = from.y = 0;
            return (to - from).normalized;
        }
        public static float GetDistance2D(Vector3 to, Vector3 from)
        {
            to.y = from.y = 0;
            return (to - from).magnitude;
        }
        public static float AngleBetween2DWithSign(Vector3 from, Vector3 to)
        {
            Vector3 v1 = new Vector3(from.x, 0, from.z);
            Vector3 v2 = new Vector3(to.x, 0, to.z);
            float angle = Vector3.Angle(v1, v2);
            Vector3 v3 = Vector3.Cross(v2, v1);
            return angle * Mathf.Sign(v3.y);
        }
        public static bool IsZero(float v, float e = EPSILON)
        {
            return Mathf.Abs(v) < EPSILON;
        }
        public static bool IsEqual(float v1, float v2, float e = EPSILON)
        {
            return Mathf.Abs(v1 - v2) < EPSILON;
        }
        public static Vector3 GetReflectedVector(Vector3 v, Vector3 n)
        {
            //Vector3 nnorm = n.normalized;
            return (v - n * (Vector3.Dot(n, v) * 2)).normalized;
        }
        public static float GetFraction(float v)
        {
            return v - (int)v;
        }
        public static Vector3 RotateVectorAroundY(Vector3 v, float angle)
        {
            Quaternion q = Quaternion.AngleAxis(-angle, Vector3.up);
            return q * v;
        }
        public static Vector3 AngleToVector2D(float angle)
        {
            return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        public static float Vector2DToAngle(Vector3 v)
        {
            float lenOfVector = Mathf.Sqrt(v.x * v.x + v.z * v.z);
            if (IsZero(lenOfVector))
            {
                return 0;
            }
            float ret = Mathf.Acos(v.x / lenOfVector);
            if (v.z < 0)
            {
                ret = Mathf.PI * 2 - ret;
            }
            return ret;
        }
        public static float NormalizeAngleZeroToTowPI(float angle)
        {
            float result = angle;
            for (; result < 0;)
            {
                result += Mathf.PI * 2;
            }
            for (; result > Mathf.PI * 2;)
            {
                result -= Mathf.PI * 2;
            }
            return result;
        }
        public static float NormalizeAngleNegPIToPI(float angle)
        {
            float result = angle;
            for (; result < -Mathf.PI;)
            {
                result += Mathf.PI * 2;
            }
            for (; result >= Mathf.PI;)
            {
                result -= Mathf.PI * 2;
            }
            return result;
        }
        public static Matrix4x4 matrixCaculation = new Matrix4x4();
        public static Vector3 TransformDirection(Vector3 f, Vector3 d)
        {
            Vector3 up = Vector3.up;
            Vector3 forward = f.normalized;
            Vector3 right = Vector3.Cross(up, forward);
            right.Normalize();
            matrixCaculation.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
            matrixCaculation.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
            matrixCaculation.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
            matrixCaculation.SetColumn(3, new Vector4(0, 0, 0, 1));
            return matrixCaculation.MultiplyVector(d);
        }
        public static Vector3 TransformPoint(Vector3 f, Vector3 basep, Vector3 p)
        {
            Vector3 up = Vector3.up;
            Vector3 forward = f.normalized;
            Vector3 right = Vector3.Cross(up, forward);
            right.Normalize();
            matrixCaculation.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
            matrixCaculation.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
            matrixCaculation.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
            matrixCaculation.SetColumn(3, new Vector4(basep.x, basep.y, basep.z, 1));
            return matrixCaculation.MultiplyPoint3x4(p);
        }
        public static float GetSign(float v)
        {
            if (v < 0)
            {
                return -1;
            }
            return 1;
        }

        public static Vector3 Vector3_0 = new Vector3(0, 0, 0);
        public static Vector3 Vector3_X = new Vector3(1, 0, 0);
        public static Vector3 Vector3_Y = new Vector3(0, 1, 0);
        public static Vector3 Vector3_Z = new Vector3(0, 0, 1);
        public static Vector3 Vector3_NX = new Vector3(-1, 0, 0);
        public static Vector3 Vector3_NY = new Vector3(0, -1, 0);
        public static Vector3 Vector3_NZ = new Vector3(0, 0, -1);
    }
}
