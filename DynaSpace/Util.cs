using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


using Point = Autodesk.DesignScript.Geometry.Point;

namespace DynaSpace
{
    public static class Util
    {
        [MultiReturn(
             "SpaceName",
             "Department",
             "DepartmentId",
             "Quantity",
             "Width",
             "Height",
             "Area",
             "Preference",
             "AdjacentSpaces",
             "AdjacentDepartments")]
        public static Dictionary<string, object> ParseData(List<object> data)
        {
            List<string> spaceNames = new List<string>();
            List<string> departments = new List<string>();
            List<int> departmentIds = new List<int>();
            List<int> quantities = new List<int>();
            List<double> widths = new List<double>();
            List<double> heights = new List<double>();
            List<double> areas = new List<double>();
            List<int> preferences = new List<int>();
            List<List<int>> adjacentSpaces = new List<List<int>>();
            List<List<int>> adjacentDepartments = new List<List<int>>();

            int stride = 12;
            int sheetHeight = data.Count / stride - 1;

            Dictionary<string, int> cols = new Dictionary<string, int>();

            List<string> columnNames = new List<string>
            {
                "SPACE ID",
                "SPACE NAME",
                "DEPARTMENT",
                "DEPARTMENT ID",
                "QUANTITY",
                "WIDTH",
                "HEIGHT",
                "AREA",
                "PREFERENCE",
                "ADJACENT SPACES",
                "ADJACENT DEPARTMENTS"
            };

            for (int i = 0; i < stride; i++)
                foreach (string columnName in columnNames)
                    if (data[i].ToString() == columnName)
                        cols.Add(columnName, i);


            for (int j = stride; j < data.Count; j += stride)
            {
                spaceNames.Add(data[j + cols["SPACE NAME"]].ToString());
                departments.Add(data[j + cols["DEPARTMENT"]].ToString());
                departmentIds.Add(int.Parse(data[j + cols["DEPARTMENT ID"]].ToString()));
                quantities.Add(int.Parse(data[j + cols["QUANTITY"]].ToString()));
                widths.Add(data[j + cols["WIDTH"]] == null ? double.NaN : double.Parse(data[j + cols["WIDTH"]].ToString()));
                heights.Add(data[j + cols["HEIGHT"]] == null ? double.NaN : double.Parse(data[j + cols["HEIGHT"]].ToString()));
                areas.Add(data[j + cols["AREA"]] == null ? double.NaN : double.Parse(data[j + cols["AREA"]].ToString()));
                preferences.Add(int.Parse(data[j + cols["PREFERENCE"]].ToString()));

                adjacentSpaces.Add(new List<int>());
                object raw = data[j + cols["ADJACENT SPACES"]];
                if (raw != null)
                {
                    string[] segments = raw.ToString().Split('.', ';');
                    foreach (string segment in segments)
                        adjacentSpaces.Last().Add(int.Parse(segment));
                }

                adjacentDepartments.Add(new List<int>());
                raw = data[j + cols["ADJACENT DEPARTMENTS"]];
                if (raw != null)
                {
                    string[] segments = raw.ToString().Split('.', ';');
                    foreach (string segment in segments)
                        adjacentDepartments.Last().Add(int.Parse(segment));
                }
            }

            //=====================================================================
            // Clean up inconsitencies and redundancies in ADJACENT SPACES
            //=====================================================================

            List<HashSet<int>> hashSets = new List<HashSet<int>>();

            for (int i = 0; i < adjacentSpaces.Count; i++)
            {
                hashSets.Add(new HashSet<int>());

                foreach (int j in adjacentSpaces[i])
                {
                    if (i < j) hashSets.Last().Add(j);
                    else hashSets[j].Add(i);
                }
            }

            for (int i = 0; i < adjacentSpaces.Count; i++)
            {
                adjacentSpaces[i] = hashSets[i].ToList();
                adjacentSpaces[i].Sort();
            }


            //========================================================================
            // Automatically compute Width, Height, or Area if missing from CSV file
            //========================================================================

            for (int i = 0; i < spaceNames.Count; i++)
            {
                if (double.IsNaN(widths[i]) && double.IsNaN(heights[i]) && !double.IsNaN(areas[i]))
                    heights[i] = widths[i] = Math.Sqrt(areas[i]);
                else if (!double.IsNaN(widths[i]) && double.IsNaN(heights[i]) && !double.IsNaN(areas[i]))
                    heights[i] = areas[i] / widths[i];
                else if (double.IsNaN(widths[i]) && !double.IsNaN(heights[i]) && !double.IsNaN(areas[i]))
                    widths[i] = areas[i] / heights[i];
                else if (!double.IsNaN(widths[i]) && !double.IsNaN(heights[i]) && double.IsNaN(areas[i]))
                    areas[i] = widths[i] * heights[i];
            }



            //=====================================================================
            // Output
            //=====================================================================

            return new Dictionary<string, object>
            {
                {"SpaceName", spaceNames},
                {"Department", departments},
                {"DepartmentId", departmentIds},
                {"Quantity", quantities},
                {"Width", widths},
                {"Height", heights},
                {"Area", areas},
                {"Preference", preferences},
                {"AdjacentSpaces", adjacentSpaces},
                {"AdjacentDepartments", adjacentDepartments}
            };
        }


        public static List<Point> TestInitPositions(int n)
        {
            List<Point> positions = new List<Point>();

            float r = 10f;

            for (int i = 0; i < n; i++)
                positions.Add(Point.ByCoordinates(r * Math.Cos((float)i / n * Math.PI * 2f), r * Math.Sin((float)i / n * Math.PI * 2f), 0f));

            return positions;
        }
    }
}
