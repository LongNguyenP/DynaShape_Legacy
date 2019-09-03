using System;

namespace DynaShape
{
    // REFERENCE:
    // This fast algorithm to compute the singular value decomposition of a 3x3 matrix is based on the technical report:
    // Computing the Singular Value Decomposition of 3 x 3 matrices with minimal branching and elementary floating point operations
    // Authors: Aleka McAdams, Andrew Selle,  Rasmus Tamstorf, Joseph Teran, Eftychios Sifakis
    // University of Wisconsin

    // The implementation below is based on the C++ CUDA implementation found at: https://github.com/ericjang/svd3

    public static class FastSvd3x3
    {
        private const float Gamma = 5.82842712474619f; // FOUR_GAMMA_SQUARED = sqrt(8)+3;
        private const float CStar = 0.923879532511287f; // cos(pi/8)
        private const float SStar = 0.38268343236509f; // sin(p/8)
        private const float Epsilon = 1e-10f;

        public static void Compute(
            // input A
            float a00, float a01, float a02,
            float a10, float a11, float a12,
            float a20, float a21, float a22,
            // output U
            out float u00, out float u01, out float u02,
            out float u10, out float u11, out float u12,
            out float u20, out float u21, out float u22,
            // output S
            out float s00, out float s11, out float s22,
            // output V
            out float v00, out float v01, out float v02,
            out float v10, out float v11, out float v12,
            out float v20, out float v21, out float v22)
        {
            // normal equations matrix
            MultAtB(a00, a01, a02,
                    a10, a11, a12,
                    a20, a21, a22,
                    a00, a01, a02,
                    a10, a11, a12,
                    a20, a21, a22,
                    out float ata00, out float ata01, out float ata02,
                    out float ata10, out float ata11, out float ata12,
                    out float ata20, out float ata21, out float ata22);

            JacobiEigenAnalysis(ref ata00,
                                ref ata10, ref ata11,
                                ref ata20, ref ata21, ref ata22,
                                out float x, out float y, out float z, out float w);

            QuatToMatrix(x, y, z, w,
                         out v00, out v01, out v02,
                         out v10, out v11, out v12,
                         out v20, out v21, out v22);

            MultAB(a00, a01, a02,
                   a10, a11, a12,
                   a20, a21, a22,
                   v00, v01, v02,
                   v10, v11, v12,
                   v20, v21, v22,
                   out float b00, out float b01, out float b02,
                   out float b10, out float b11, out float b12,
                   out float b20, out float b21, out float b22);

            // sort singular values and find V
            SortSingularValues(ref b00, ref b01, ref b02,
                               ref b10, ref b11, ref b12,
                               ref b20, ref b21, ref b22,
                               ref v00, ref v01, ref v02,
                               ref v10, ref v11, ref v12,
                               ref v20, ref v21, ref v22);

            // QR decomposition
            QRDecomposition(b00, b01, b02,
                            b10, b11, b12,
                            b20, b21, b22,
                            out u00, out u01, out u02,
                            out u10, out u11, out u12,
                            out u20, out u21, out u22,
                            out       s00, out float s01, out float s02,
                            out float s10, out       s11, out float s12,
                            out float s20, out float s21, out       s22);
        }

        private static float InvSqrt(float a) => (float)(1.0 / Math.Sqrt(a));

        private static void Swap(bool c, ref float x, ref float y)
        {
            // used in step 2
            float z = x;
            x = c ? y : x;
            y = c ? z : y;
        }

        private static void NegativeSwap(bool c, ref float x, ref float y)
        {
            // used in step 2 and 3
            float z = -x;
            x = c ? y : x;
            y = c ? z : y;
        }

        // matrix multiplication M = A * B
        private static void MultAB(
            float a00, float a01, float a02,
            float a10, float a11, float a12,
            float a20, float a21, float a22,
            float b00, float b01, float b02,
            float b10, float b11, float b12,
            float b20, float b21, float b22,
            out float m00, out float m01, out float m02,
            out float m10, out float m11, out float m12,
            out float m20, out float m21, out float m22)
        {

            m00 = a00 * b00 + a01 * b10 + a02 * b20;
            m01 = a00 * b01 + a01 * b11 + a02 * b21;
            m02 = a00 * b02 + a01 * b12 + a02 * b22;
            m10 = a10 * b00 + a11 * b10 + a12 * b20;
            m11 = a10 * b01 + a11 * b11 + a12 * b21;
            m12 = a10 * b02 + a11 * b12 + a12 * b22;
            m20 = a20 * b00 + a21 * b10 + a22 * b20;
            m21 = a20 * b01 + a21 * b11 + a22 * b21;
            m22 = a20 * b02 + a21 * b12 + a22 * b22;
        }

        // matrix multiplication M = Transpose[A] * B
        private static void MultAtB(
            float a00, float a01, float a02,
            float a10, float a11, float a12,
            float a20, float a21, float a22,
            float b00, float b01, float b02,
            float b10, float b11, float b12,
            float b20, float b21, float b22,
            out float m00, out float m01, out float m02,
            out float m10, out float m11, out float m12,
            out float m20, out float m21, out float m22)
        {
            m00 = a00 * b00 + a10 * b10 + a20 * b20;
            m01 = a00 * b01 + a10 * b11 + a20 * b21;
            m02 = a00 * b02 + a10 * b12 + a20 * b22;
            m10 = a01 * b00 + a11 * b10 + a21 * b20;
            m11 = a01 * b01 + a11 * b11 + a21 * b21;
            m12 = a01 * b02 + a11 * b12 + a21 * b22;
            m20 = a02 * b00 + a12 * b10 + a22 * b20;
            m21 = a02 * b01 + a12 * b11 + a22 * b21;
            m22 = a02 * b02 + a12 * b12 + a22 * b22;
        }


        private static void QuatToMatrix(
            float x, float y, float z, float w,
            out float m00, out float m01, out float m02,
            out float m10, out float m11, out float m12,
            out float m20, out float m21, out float m22)
        {
            float qxx = x * x;
            float qyy = y * y;
            float qzz = z * z;
            float qxz = x * z;
            float qxy = x * y;
            float qyz = y * z;
            float qwx = w * x;
            float qwy = w * y;
            float qwz = w * z;
            m00 = 1f - 2f * (qyy + qzz);
            m01 =      2f * (qxy - qwz);
            m02 =      2f * (qxz + qwy);
            m10 =      2f * (qxy + qwz);
            m11 = 1f - 2f * (qxx + qzz);
            m12 =      2f * (qyz - qwx);
            m20 =      2f * (qxz - qwy);
            m21 =      2f * (qyz + qwx);
            m22 = 1f - 2f * (qxx + qyy);
        }


        private static void ApproximateGivensQuaternion(float a00, float a01, float a11, out float ch, out float sh)
        {
            //Given givens angle computed by approximateGivensAngles, compute the corresponding rotation quaternion.
            ch = 2 * (a00 - a11);
            sh = a01;
            bool b = Gamma * sh * sh < ch * ch;
            float w = InvSqrt(ch * ch + sh * sh);
            ch = b ? w * ch : CStar;
            sh = b ? w * sh : SStar;
        }

        private static void JacobiConjugation(
            int x, int y, int z,
            ref float s00,
            ref float s10, ref float s11,
            ref float s20, ref float s21, ref float s22,
            float[] qV)
        {
            ApproximateGivensQuaternion(s00, s10, s11, out float ch, out float sh);

            float scale = ch * ch + sh * sh;
            float a = (ch * ch - sh * sh) / scale;
            float b = (2 * sh * ch) / scale;

            // make temp copy of S
            float _s00 = s00;
            float _s10 = s10;
            float _s11 = s11;
            float _s20 = s20;
            float _s21 = s21;
            float _s22 = s22;

            // perform conjugation S = Q'*S*Q
            // Q already implicitly solved from a, b
            s00 =  a * ( a * _s00 + b * _s10) + b * ( a * _s10 + b * _s11);
            s10 =  a * (-b * _s00 + a * _s10) + b * (-b * _s10 + a * _s11);
            s11 = -b * (-b * _s00 + a * _s10) + a * (-b * _s10 + a * _s11);
            s20 =  a * _s20 + b * _s21;
            s21 = -b * _s20 + a * _s21;
            s22 = _s22;

            // update cumulative rotation qV
            float[] tmp = { qV[0] * sh, qV[1] * sh, qV[2] * sh };

            sh *= qV[3];

            qV[0] *= ch;
            qV[1] *= ch;
            qV[2] *= ch;
            qV[3] *= ch;

            // (x,y,z) corresponds to ((0,1,2),(1,2,0),(2,0,1))
            // for (p,q) = ((0,1),(1,2),(0,2))
            qV[z] += sh;
            qV[3] -= tmp[z]; // w
            qV[x] += tmp[y];
            qV[y] -= tmp[x];

            // re-arrange matrix for next iteration
            _s00 = s11;
            _s10 = s21;
            _s11 = s22;
            _s20 = s10;
            _s21 = s20;
            _s22 = s00;
            s00 = _s00;
            s10 = _s10;
            s11 = _s11;
            s20 = _s20;
            s21 = _s21;
            s22 = _s22;
        }

        private static float NormSquared(float x, float y, float z) => x * x + y * y + z * z;

        // finds transformation that diagonalizes a symmetric matrix
        private static void JacobiEigenAnalysis(
            ref float s00,
            ref float s10, ref float s11,
            ref float s20, ref float s21, ref float s22,
            out float q0, out float q1, out float q2, out float q3)
        {
            // follow same indexing convention as GLM
            float[] qV = {0f, 0f, 0f, 1f};

            for (int i = 0; i < 4; i++)
            {
                // we wish to eliminate the maximum off-diagonal element on every iteration, but cycling over all 3 possible rotations
                // in fixed order (p,q) = (1,2) , (2,3), (1,3) still retains asymptotic convergence
                JacobiConjugation(0, 1, 2, ref s00, ref s10, ref s11, ref s20, ref s21, ref s22, qV); // p,q = 0,1
                JacobiConjugation(1, 2, 0, ref s00, ref s10, ref s11, ref s20, ref s21, ref s22, qV); // p,q = 1,2
                JacobiConjugation(2, 0, 1, ref s00, ref s10, ref s11, ref s20, ref s21, ref s22, qV); // p,q = 0,2
            }

            q0 = qV[0];
            q1 = qV[1];
            q2 = qV[2];
            q3 = qV[3];
        }

        private static void SortSingularValues(
            // matrix that we want to decompose
            ref float b00, ref float b01, ref float b02,
            ref float b10, ref float b11, ref float b12,
            ref float b20, ref float b21, ref float b22,
            // sort V simultaneously
            ref float v00, ref float v01, ref float v02,
            ref float v10, ref float v11, ref float v12,
            ref float v20, ref float v21, ref float v22)
        {
            float rho1 = NormSquared(b00, b10, b20);
            float rho2 = NormSquared(b01, b11, b21);
            float rho3 = NormSquared(b02, b12, b22);
            bool c = rho1 < rho2;
            NegativeSwap(c, ref b00, ref b01);
            NegativeSwap(c, ref v00, ref v01);
            NegativeSwap(c, ref b10, ref b11);
            NegativeSwap(c, ref v10, ref v11);
            NegativeSwap(c, ref b20, ref b21);
            NegativeSwap(c, ref v20, ref v21);
            Swap(c, ref rho1, ref rho2);
            c = rho1 < rho3;
            NegativeSwap(c, ref b00, ref b02);
            NegativeSwap(c, ref v00, ref v02);
            NegativeSwap(c, ref b10, ref b12);
            NegativeSwap(c, ref v10, ref v12);
            NegativeSwap(c, ref b20, ref b22);
            NegativeSwap(c, ref v20, ref v22);
            Swap(c, ref rho1, ref rho3);
            c = rho2 < rho3;
            NegativeSwap(c, ref b01, ref b02);
            NegativeSwap(c, ref v01, ref v02);
            NegativeSwap(c, ref b11, ref b12);
            NegativeSwap(c, ref v11, ref v12);
            NegativeSwap(c, ref b21, ref b22);
            NegativeSwap(c, ref v21, ref v22);
        }

        private static void QRGivensQuaternion(float a1, float a2, out float ch, out float sh)
        {
            // a1 = pivot point on diagonal
            // a2 = lower triangular entry we want to annihilate
            float rho = (float)Math.Sqrt(a1 * a1 + a2 * a2);

            sh = rho > Epsilon ? a2 : 0;
            ch = Math.Abs(a1) + Math.Max(rho, Epsilon);
            bool b = a1 < 0;
            Swap(b, ref sh, ref ch);
            float w = InvSqrt(ch * ch + sh * sh);
            ch *= w;
            sh *= w;
        }

        private static void QRDecomposition(
            // matrix that we want to decompose
            float b00, float b01, float b02,
            float b10, float b11, float b12,
            float b20, float b21, float b22,
            // output Q
            out float q00, out float q01, out float q02,
            out float q10, out float q11, out float q12,
            out float q20, out float q21, out float q22,
            // output R
            out float r00, out float r01, out float r02,
            out float r10, out float r11, out float r12,
            out float r20, out float r21, out float r22)
        {
            // first givens rotation (ch,0,0,sh)
            QRGivensQuaternion(b00, b10, out float ch1, out float sh1);
            float a = 1f - 2f * sh1 * sh1;
            float c = 2f * ch1 * sh1;
            // apply B = Q' * B
            r00 =  a * b00 + c * b10;
            r01 =  a * b01 + c * b11;
            r02 =  a * b02 + c * b12;
            r10 = -c * b00 + a * b10;
            r11 = -c * b01 + a * b11;
            r12 = -c * b02 + a * b12;
            r20 =  b20;
            r21 =  b21;
            r22 =  b22;

            // second givens rotation (ch,0,-sh,0)
            QRGivensQuaternion(r00, r20, out float ch2, out float sh2);
            a = 1f - 2f * sh2 * sh2;
            c = 2f * ch2 * sh2;
            // apply B = Q' * B;
            b00 =  a * r00 + c * r20;
            b01 =  a * r01 + c * r21;
            b02 =  a * r02 + c * r22;
            b10 =  r10;
            b11 =  r11;
            b12 =  r12;
            b20 = -c * r00 + a * r20;
            b21 = -c * r01 + a * r21;
            b22 = -c * r02 + a * r22;

            // third givens rotation (ch,sh,0,0)
            QRGivensQuaternion(b11, b21, out float ch3, out float sh3);
            a = 1f - 2f * sh3 * sh3;
            c = 2f * ch3 * sh3;
            // R is now set to desired value
            r00 =  b00;
            r01 =  b01;
            r02 =  b02;
            r10 =  a * b10 + c * b20;
            r11 =  a * b11 + c * b21;
            r12 =  a * b12 + c * b22;
            r20 = -c * b10 + a * b20;
            r21 = -c * b11 + a * b21;
            r22 = -c * b12 + a * b22;

            // construct the cumulative rotation Q = Q1 * Q2 * Q3
            // the number of floating point operations for three quaternion multiplications
            // is more or less comparable to the explicit form of the joined matrix.
            // certainly more memory-efficient!
            float sh01 = sh1 * sh1;
            float sh11 = sh2 * sh2;
            float sh21 = sh3 * sh3;

            q00 = (-1f + 2f * sh01) * (-1f + 2f * sh11);
            q01 = 4f * ch2 * ch3 * (-1f + 2f * sh01) * sh2 * sh3 + 2f * ch1 * sh1 * (-1f + 2f * sh21);
            q02 = 4f * ch1 * ch3 * sh1 * sh3 - 2f * ch2 * (-1f + 2f * sh01) * sh2 * (-1f + 2f * sh21);

            q10 = 2f * ch1 * sh1 * (1f - 2f * sh11);
            q11 = -8f * ch1 * ch2 * ch3 * sh1 * sh2 * sh3 + (-1f + 2f * sh01) * (-1f + 2f * sh21);
            q12 = -2f * ch3 * sh3 + 4f * sh1 * (ch3 * sh1 * sh3 + ch1 * ch2 * sh2 * (-1f + 2f * sh21));

            q20 = 2f * ch2 * sh2;
            q21 = 2f * ch3 * (1f - 2f * sh11) * sh3;
            q22 = (-1f + 2f * sh11) * (-1f + 2f * sh21);
        }
    }
}