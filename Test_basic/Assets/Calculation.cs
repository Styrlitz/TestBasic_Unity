using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Calculation : MonoBehaviour
{
    const int numberOfPoints = 5;   // Кількість заданих точок
    const int graphWidth = 1000;    // Ширина графіку
    const int graphHeight = 700;    // Висота графіку

    const int zeroOffsetX = 200;    // Зсув від краю графіку до 0 по Х
    const int zeroOffsetY = 200;    // Зсув від краю графіку до 0 по Y

    const int sixOffsetX = 500;     // Зсув від краю графіку до 6 по Х
    const int sixOffsetY = 200;     // Зсув від краю графіку до 6 по Y


    const int basis = 4;

    private RectTransform graphContainer;

    [SerializeField] private GameObject circle;         // Префаб заданої точки
    [SerializeField] private GameObject circles_parent; // Об'єкт є parent всіх префабів заданих точок
    List<GameObject> circles = new List<GameObject>();  // Список заданих точок

    float[] result;
    float[,] xyTable, matrix;

    int y_coord, y_previous;
    int y_previous_lag;

    [SerializeField] Texture2D graph_texture;

    void Start()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();

        for (int i = 1; i < 6; i++)
        {
            GameObject circle_temp = Instantiate(circle, transform.position, transform.rotation);
            circle_temp.transform.SetParent(circles_parent.transform);
            circle_temp.transform.localPosition = new Vector3(50 * i + zeroOffsetX, zeroOffsetY, 0);
            circle_temp.transform.localScale = new Vector3(1,1,1);
            circles.Add(circle_temp);
        }
    }

    void DrawGraph()
    {
        float x;
        Clean();

        for (int i = zeroOffsetX; i < sixOffsetX; i++)  // Від 0 до 6
        {
            x = (i - 200.0f)*(16.0f)/(800.0f);          // Пікселі в координати
            CalculateY(x, i);
            InterpolateLagrangePolynomial(x, i, xyTable, 5);
        }

        graph_texture.Apply();
    }

    void Clean()
    {
        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                graph_texture.SetPixel(i, j, Color.clear);
            }
        }
        graph_texture.Apply();
    }

    void CalculateY(float x, int x_coord)
    {        
        float y = 0;
        for (int i =0; i < basis; i++)
        {
            y += result[i] * Mathf.Pow(x, i);
        }

        y_coord = Mathf.RoundToInt((y * (1000-200))/16 + 200);

        if (x != 0 && Mathf.Abs(y_previous - y_coord) > 1)
        {
            for (int i = 1; i < Mathf.Abs(y_previous - y_coord); i++)
            {
                if (y_previous > y_coord)
                {
                    graph_texture.SetPixel(x_coord, y_coord + i, Color.red);
                }
                else
                {
                    graph_texture.SetPixel(x_coord, y_coord - i, Color.red);
                }
            }
        }

        y_previous = y_coord;
        graph_texture.SetPixel(x_coord, y_coord, Color.red);
    }

    public void CalculateClick()
    {
        xyTable = new float[2, numberOfPoints];
        int arr_index = 0;

        foreach (var circle in circles)
        {
            xyTable[0, arr_index] = (circle.transform.localPosition.x - 200.0f)*(16.0f)/(800.0f);
            xyTable[1, arr_index] = (circle.transform.localPosition.y)*(14.0f)/(700.0f) - 4.0f;
            arr_index++;            
        }

        matrix = MakeSystem(xyTable, basis);
        result = Gauss(matrix, basis, basis + 1);

        if (result == null)
        {
            return;
        }

        DrawGraph();
    }

    float InterpolateLagrangePolynomial (float x, int x_coord, float[,] xyTable, int size)
    {
        float lagrangePol = 0;

        for (int i = 0; i < size; i++)
        {
            float basicsPol = 1;

            for (int j = 0; j < size; j++)
            {
                if (j != i)
                {
                    basicsPol *= (x - xyTable[0,j])/(xyTable[0,i] - xyTable[0,j]);
                }
            }

            lagrangePol += basicsPol * xyTable[1, i];
        }

        y_coord = Mathf.RoundToInt((lagrangePol * (1000-200))/16 + 200);

        if (x != 0 && Mathf.Abs(y_previous_lag - y_coord) > 1)
        {
            for (int i = 1; i < Mathf.Abs(y_previous_lag - y_coord); i++)
            {
                if (y_previous_lag > y_coord)
                {
                    graph_texture.SetPixel(x_coord, y_coord + i, Color.blue);
                }
                else
                {
                    graph_texture.SetPixel(x_coord, y_coord - i, Color.blue);
                }
            }
        }

        y_previous_lag = y_coord;
        graph_texture.SetPixel(x_coord, y_coord, Color.blue);
        return lagrangePol;
    }

    float[] Gauss(float[,] matrix, int rowCount, int colCount)
    {
        int i;
        int[] mask = new int[colCount - 1];

        for (i = 0; i < colCount - 1; i++)
        {
            mask[i] = i;
        }
        
        if (GaussDirectPass(ref matrix, ref mask, colCount, rowCount))
        {
            float[] answer = GaussReversePass(ref matrix, mask, colCount, rowCount);
            return answer;
        }
        else
        {
            return null;
        }
    }

    bool GaussDirectPass(ref float[,] matrix, ref int[] mask, int colCount, int rowCount)
    {
        int i, j, k, maxId, tmpInt;
        float maxVal, tempFloat;

        for (i = 0; i < rowCount; i++)
        {
            maxId = i;
            maxVal = matrix[i,i];

            for (j = i + 1; j < colCount - 1; j++)
            {
                if (Mathf.Abs(maxVal) < Mathf.Abs(matrix[i, j]))
                {
                    maxVal = matrix[i, j];
                    maxId = j;
                }
            }

            if (maxVal == 0)
            {
                return false;
            }

            if (i != maxId)
            {
                for (j = 0; j < rowCount; j++)
                {
                    tempFloat = matrix[j,i];
                    matrix[j, i] = matrix[j, maxId];
                    matrix[j, maxId] = tempFloat;
                }
                tmpInt = mask[i];
                mask[i] = mask[maxId];
                mask[maxId] = tmpInt;
            }

            for (j = 0; j < colCount; j++)
            {
                matrix[i, j] /= maxVal;
            }

            for (j = i + 1; j < rowCount; j++)
            {
                float tempMn = matrix[j, i];
                for (k = 0; k < colCount; k++)
                {
                    matrix[j, k] -= matrix[i, k] * tempMn;
                }
            }
        }

        return true;
    }

    float[] GaussReversePass(ref float[,] matrix, int[] mask, int colCount, int rowCount)
    {
        int i,j,k;
        for (i = rowCount - 1; i >= 0; i--)
        {
            for (j = i - 1; j >= 0; j--)
            {
                float tempMn = matrix[j,i];
                for (k = 0; k < colCount; k++)
                {
                    matrix[j, k] -= matrix[i,k] * tempMn;
                }
            }
        }
        float[] answer = new float[rowCount];
        for (i = 0; i < rowCount; i++)
        {
            answer[mask[i]] = matrix[i, colCount - 1];
        }
        return answer;
    }

    float[,] MakeSystem(float[,] xyTable, int basis)
    {
        float[,] matrix = new float[basis, basis+1];

        for (int i = 0; i < basis; i++)
        {
            for(int j = 0; j < basis; j++)
            {
                float sumA = 0, sumB = 0;
                for (int k = 0; k < numberOfPoints; k++)
                {
                    sumA += Mathf.Pow(xyTable[0, k], i) * Mathf.Pow(xyTable[0, k], j);
                    sumB += xyTable[1, k] * Mathf.Pow(xyTable[0, k], i);
                }
                matrix[i, j] = sumA;
                matrix[i, basis] = sumB;
            }
        }
        return matrix;
    }

    void OnApplicationQuit()
    {
        Clean();
    }
}
