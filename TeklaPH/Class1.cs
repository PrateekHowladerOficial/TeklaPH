using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Tekla.Structures.Solid;
using static Tekla.Structures.Model.Position;
using TSG = Tekla.Structures.Geometry3d;
using static TeklaPH.Faces;

namespace TeklaPH
{
    public class Faces
    {
        public class Face_
        {
            public Face Face { get; set; }
            public Vector Vector { get; set; }
            public void face_(Face face, Vector vector)
            {
                Face = face;
                Vector = vector;
            }
        }
        public static List<Face_> Get_faces(Part beam)
        {

            Solid solid = beam.GetSolid();
            FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
            List<Face_> faces = new List<Face_>();
            while (faceEnumerator.MoveNext())
            {

                Face face = faceEnumerator.Current as Face;
                Vector vector = face.Normal;
                faces.Add(new Face_ { Face = face, Vector = vector });

            }

            return faces;
        }
        public static List<Face_> Get_faces(Part beam, bool raw)
        {

            Solid solid = (raw) ? beam.GetSolid(0) : beam.GetSolid();
            FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
            List<Face_> faces = new List<Face_>();
            while (faceEnumerator.MoveNext())
            {

                Face face = faceEnumerator.Current as Face;
                Vector vector = face.Normal;
                faces.Add(new Face_ { Face = face, Vector = vector });

            }

            return faces;
        }
        public static double CalculateFaceArea(Face_ face)
        {
            ArrayList facePoints = Faces.Get_Points(face.Face); // Assuming this method gets the list of points of the face

            if (facePoints.Count < 3)
                return 0.0; // A face must have at least 3 points to form a polygon

            double totalArea = 0.0;
            Point basePoint = facePoints[0] as Point;

            // Iterate through the face points and form triangles with the base point
            for (int i = 1; i < facePoints.Count - 1; i++)
            {
                Point point1 = facePoints[i] as Point;
                Point point2 = facePoints[i + 1] as Point;

                // Calculate the area of the triangle formed by basePoint, point1, and point2
                totalArea += CalculateTriangleArea(basePoint, point1, point2);
            }

            return totalArea;
        }
        public static double CalculateFaceArea(Face face)
        {
            ArrayList facePoints = Get_Points(face); // Assuming this method gets the list of points of the face

            if (facePoints.Count < 3)
                return 0.0; // A face must have at least 3 points to form a polygon

            double totalArea = 0.0;
            Point basePoint = facePoints[0] as Point;

            // Iterate through the face points and form triangles with the base point
            for (int i = 1; i < facePoints.Count - 1; i++)
            {
                Point point1 = facePoints[i] as Point;
                Point point2 = facePoints[i + 1] as Point;

                // Calculate the area of the triangle formed by basePoint, point1, and point2
                totalArea += CalculateTriangleArea(basePoint, point1, point2);
            }

            return totalArea;
        }
        public static double CalculateTriangleArea(Point p1, Point p2, Point p3)
        {
            // Create vectors representing two sides of the triangle
            Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

            // The area of the triangle is half the magnitude of the cross product of the vectors
            Vector crossProduct = v1.Cross(v2);
            double area = 0.5 * crossProduct.GetLength();
            return area;
        }
        public static ArrayList Get_Points(Face face)
        {
            ArrayList points = new ArrayList();
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }
            return points;
        }
        public static void GetFaceAxes(Face face, out Vector xAxis, out Vector yAxis)
        {
            Vector normalVector;
            // Get the loop vertices of the face to extract points
            ArrayList vertices = Get_Points(face);

            if (vertices == null || vertices.Count < 3)
            {
                throw new ArgumentException("The face does not have enough vertices to define axes.");
            }

            // Select three distinct points to define the plane and axes
            Point point1 = vertices[0] as Point;
            Point point2 = vertices[1] as Point;
            Point point3 = vertices[2] as Point;

            // Define the X-axis vector as the vector between point1 and point2
            xAxis = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            xAxis.Normalize();

            // Define another vector on the face
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of xAxis and vector2)
            normalVector = Vector.Cross(xAxis, vector2);
            normalVector.Normalize();

            // Define the Y-axis vector as the cross product of the normal vector and X-axis vector
            yAxis = Vector.Cross(normalVector, xAxis);
            yAxis.Normalize();
        }
        public static GeometricPlane ConvertFaceToGeometricPlane(Face face)
        {
            ArrayList points = new ArrayList();
            // Get the edges from the face (since 'Points' is not available)
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }

            Point point1 = points[0] as Point;
            Point point2 = points[1] as Point;
            Point point3 = points[2] as Point;



            if (point1 == null || point2 == null || point3 == null)
            {
                throw new ArgumentException("The face does not have sufficient points to define a plane.");
            }

            // Create vectors from the points
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of the two vectors)
            Vector normalVector = Vector.Cross(vector1, vector2);
            normalVector.Normalize();

            // Create the geometric plane using point1 and the normal vector
            GeometricPlane geometricPlane = new GeometricPlane(point1, normalVector);

            return geometricPlane;
        }
        public static double CalculateDistanceBetweenFaces(Face face1, Face face2)
        {
            // Get the loop vertices of both faces to extract points
            ArrayList face1Vertices = Get_Points(face1);
            ArrayList face2Vertices = Get_Points(face2);

            if (face1Vertices == null || face1Vertices.Count == 0 || face2Vertices == null || face2Vertices.Count == 0)
            {
                throw new ArgumentException("One or both faces do not have vertices.");
            }

            // Initialize the minimum distance to a large value
            double minDistance = double.MaxValue;

            // Loop through all points on face1 and face2 and calculate the distance between each pair
            foreach (Point p1 in face1Vertices)
            {
                foreach (Point p2 in face2Vertices)
                {
                    double distance = Distance.PointToPoint(p1, p2);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
        }
        public bool IsLineSegmentIntersectingFace(LineSegment lineSegment, Face face_)
        {
            Line line = new Line();

            // Create a plane from the first 3 points of the face
            GeometricPlane facePlane = ConvertFaceToGeometricPlane(face_);

            // Find the intersection of the line with the plane of the face
            Point intersectionPoint = Intersection.LineToPlane(new Tekla.Structures.Geometry3d.Line(lineSegment.Point1, lineSegment.Point2), facePlane);

            // If no intersection point, the line is parallel to the plane
            if (intersectionPoint == null)
            {
                return false;
            }

            // Check if the intersection point lies on the line segment
            if (!line.IsPointOnLineSegment(intersectionPoint, lineSegment))
            {
                return false;
            }
            List<Point> points = new List<Point>();
            foreach (var v in Get_Points(face_))
            {
                points.Add(v as Point);
            }
            // Now check if the intersection point lies within the face polygon
            return line.IsPointInPolygon(intersectionPoint, points);
        }
    }
    public class Line
    {
        public static Point MidPoint(Point point, Point point1)
        {
            Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
            return mid;
        }
        public static Point FindPointOnLine(Point startPoint, Point secondPoint, double distance)
        {
            if (distance == 0)
                return startPoint;
            // Step 1: Calculate the direction vector from startPoint to secondPoint
            Vector direction = new Vector(
                secondPoint.X - startPoint.X,
                secondPoint.Y - startPoint.Y,
                secondPoint.Z - startPoint.Z
            );

            // Step 2: Normalize the direction vector
            direction.Normalize();

            // Step 3: Scale the direction vector by the distance
            Vector scaledVector = new Vector(
                direction.X * distance,
                direction.Y * distance,
                direction.Z * distance
            );

            // Step 4: Calculate the new point by adding the scaled vector to the start point
            Point newPoint = new Point(
                startPoint.X + scaledVector.X,
                startPoint.Y + scaledVector.Y,
                startPoint.Z + scaledVector.Z
            );

            return newPoint;
        }
        public Point FindPerpendicularIntersection(Point line1Start, Point line1End, Point pointOnLine1, Point line2Start, Point line2End)
        {
            Vector vector = new Vector(line1Start.X - line1End.X, line1Start.Y - line1End.Y, line1Start.Z - line1End.Z);
            GeometricPlane plane = new GeometricPlane(pointOnLine1, vector);
            Point intersection = Intersection.LineToPlane(new Tekla.Structures.Geometry3d.Line(line2Start, line2End), plane);
            return intersection;
        }
        public LineSegment ChangeLinesegment(LineSegment segment, double d)
        {
            Point p1 = FindPointOnLine(segment.StartPoint, segment.EndPoint, d * -1),
                p2 = FindPointOnLine(segment.EndPoint, segment.StartPoint, d * -1);
            return new LineSegment(p1, p2);
        }
        public bool IsPointOnLineSegment(Point point, LineSegment segment)
        {
            double totalDistance = Distance.PointToPoint(point, segment.Point1) + Distance.PointToPoint(point, segment.Point2);
            return System.Math.Abs(totalDistance - segment.Length()) < 1e-6; // Adding small tolerance
        }
        public bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            bool isInside = false;
            int polygonCount = polygon.Count;

            // Iterate over each edge of the polygon
            for (int i = 0, j = polygonCount - 1; i < polygonCount; j = i++)
            {
                Point vertex1 = polygon[i];
                Point vertex2 = polygon[j];

                // Check if the ray from point intersects the polygon edge (vertex1, vertex2)
                bool intersect = ((vertex1.Y > point.Y) != (vertex2.Y > point.Y)) &&
                                 (point.X < (vertex2.X - vertex1.X) * (point.Y - vertex1.Y) / (vertex2.Y - vertex1.Y) + vertex1.X);

                if (intersect)
                {
                    isInside = !isInside; // Toggle the state
                }
            }

            return isInside;
        }
        public static LineSegment FindPerpendicularLineSegment(Tekla.Structures.Geometry3d.Line l1, GeometricPlane gp1, Point p1, double length)
        {
            Tekla.Structures.Geometry3d.Line line = Projection.LineToPlane(l1, gp1);
            // Step 1: Get the direction vector of line l1
            Vector directionL1 = line.Direction.GetNormal();

            // Step 2: Find the cross product of the direction vector of the line and the normal of the geometric plane
            Vector perpendicularDirection = directionL1.Cross(gp1.GetNormal());

            // Step 3: Scale the perpendicular direction to the desired length (100 units)
            Vector scaledPerpendicular = new Vector(
                perpendicularDirection.X * (length / 2),
                perpendicularDirection.Y * (length / 2),
                perpendicularDirection.Z * (length / 2)
            );

            // Step 4: Find the start and end points of the perpendicular line segment
            Point startPoint = p1 - scaledPerpendicular; // Half the length in one direction
            Point endPoint = p1 + scaledPerpendicular;   // Half the length in the opposite direction

            // Step 5: Return the perpendicular line segment
            return new LineSegment(startPoint, endPoint);
        }

    }
    public class Input
    {
        public static List<double> InputConverter(string input)
        {
            if (input == "")
                return null;
            string[] hold = input.Split(' ');
            List<double> output = new List<double>();
            foreach (string s in hold)
            {
                if (s.Contains('*'))
                {
                    string[] strings = s.Split('*');
                    for (int i = 0; i < int.Parse(strings[0]); i++)
                    {
                        output.Add(double.Parse(strings[1]));
                    }
                }
                else
                {
                    output.Add(double.Parse(s));
                }
            }
            return output;
        }
    }
    public class GeoPlane
    {
        public static GeometricPlane CreatePlaneFromThreePoints(Point point1, Point point2, Point point3)
        {
            // Calculate two direction vectors on the plane
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector of the plane by taking the cross product of the two direction vectors
            Vector normalVector = vector1.Cross(vector2);

            // Create and return the geometric plane using the first point and the normal vector
            GeometricPlane plane = new GeometricPlane(point1, normalVector);

            return plane;
        }
        public static double DistanceBetweenParallelPlanes(GeometricPlane plane1, GeometricPlane plane2)
        {


            // Planes are parallel if their normal vectors are either identical or opposite
            if (!TSG.Parallel.PlaneToPlane(plane1, plane2))
            {
                throw new ArgumentException("The planes are not parallel.");
            }

            Point point = new Point(0, 0, 0);
            Point p1 = Projection.PointToPlane(point, plane1),
                p2 = Projection.PointToPlane(point, plane2);

            return Distance.PointToPoint(p1, p2);
        }
    }
    public class Fitting
    {
        public Face_ Edge_fitting(Part beam1, Part beam2, double gap)
        {
            Faces faces = new Faces();
            Line line = new Line();
            List<Face_> beam1_faces = Get_faces(beam1);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;

            Point mid = Line.MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[0].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[10].Face)))
            {
                face = beam1_faces[0];
            }
            else
            {
                face = beam1_faces[10];
            }
            Tekla.Structures.Model.Fitting fitting = new Tekla.Structures.Model.Fitting();
            fitting.Plane = new Plane();
            Vector vector = face.Vector;
            Point point = Get_Points(face.Face)[0] as Point;

            Point point1 = new Point(point.X + gap * vector.X, point.Y + gap * vector.Y, point.Z + gap * vector.Z);
            fitting.Plane.Origin = point1;

            GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();

            return face;
        }
        private Face_ Web_Fitting(Part beam1, Part beam2)
        {
            Faces faces = new Faces();
            Line line = new Line();
            List<Face_> beam1_faces = Get_faces(beam1);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;

            Point mid = Line.MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[2].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[8].Face)))
            {
                face = beam1_faces[2];
            }
            else
            {
                face = beam1_faces[8];
            }
            Tekla.Structures.Model.Fitting fitting = new Tekla.Structures.Model.Fitting();
            fitting.Plane = new Plane();
            Vector vector = face.Vector;
            Point point = Get_Points(face.Face)[0] as Point;

            fitting.Plane.Origin = point;

            GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();

            return face;
        }
        public static Face_ Web_Fitting(Part beam1, Part beam2, double gap)
        {
            Faces faces = new Faces();
            Line line = new Line();
            List<Face_> beam1_faces = Get_faces(beam1, true);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;

            Point mid = Line.MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[2].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[8].Face)))
            {
                face = beam1_faces[2];
            }
            else
            {
                face = beam1_faces[8];
            }
            Tekla.Structures.Model.Fitting fitting = new Tekla.Structures.Model.Fitting();
            fitting.Plane = new Plane();
            Vector vector = face.Vector;
            Point point = (Get_Points(face.Face)[0] as Point) + face.Vector * gap;

            fitting.Plane.Origin = point;

            GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();

            return face;
        }
        private void BeamBooleanCut(Part beam1, Part beam2, double clearance, double gap, double thickness)
        {
            Faces faces = new Faces();
            Line line = new Line();
            List<Face_> beam1_faces = Get_faces(beam1),
                beam2_faces = Get_faces(beam2);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;
            int edgeIndex = -1;
            Point mid = Line.MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[2].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[8].Face)))
            {
                face = beam1_faces[2];
                edgeIndex = 0;
            }
            else
            {
                face = beam1_faces[8];
                edgeIndex = 10;
            }
            Point holdIntersection = Intersection.LineToPlane(new Tekla.Structures.Geometry3d.Line(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point), ConvertFaceToGeometricPlane(face.Face));


            Point p1 = Projection.PointToPlane(holdIntersection, ConvertFaceToGeometricPlane(beam2_faces[5].Face)),
                p2 = Projection.PointToPlane(holdIntersection, ConvertFaceToGeometricPlane(beam2_faces[11].Face));
            LineSegment lineSegment = new LineSegment(p1, p2);
            lineSegment = line.ChangeLinesegment(lineSegment, clearance);
            foreach (int n in new List<int> { 5, 11 })
            {
                if (faces.IsLineSegmentIntersectingFace(lineSegment, beam1_faces[n].Face))
                {
                    Point point1 = Projection.PointToPlane(holdIntersection, ConvertFaceToGeometricPlane(beam1_faces[n].Face)),
                        point2 = Projection.PointToPlane(point1, ConvertFaceToGeometricPlane(beam1_faces[edgeIndex].Face));
                    Beam beam = new Beam();
                    beam.StartPoint = point1; beam.EndPoint = TeklaPH.Line.FindPointOnLine(point2, point1, gap * -1);
                    double dis = CalculateDistanceBetweenFaces(beam2_faces[0].Face, beam2_faces[10].Face);
                    beam.Profile.ProfileString = "PLT" + thickness * 2 + "*" + dis * 1.5;
                    beam.Position.Depth = Position.DepthEnum.MIDDLE;
                    beam.Position.Plane = Position.PlaneEnum.MIDDLE;
                    beam.Position.Rotation = Position.RotationEnum.FRONT;
                    beam.Class = BooleanPart.BooleanOperativeClassName;
                    beam.Insert();
                    BooleanPart booleanPart = new BooleanPart();
                    booleanPart.Father = beam2;
                    booleanPart.SetOperativePart(beam);
                    booleanPart.Insert();

                    beam.Delete();
                }
            }
        }
        private void SameProfileEdgeJoint(Part part1, Part part2)
        {
            Faces faces = new Faces();
            Line line = new Line();
            GeoPlane geoPlane = new GeoPlane();
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);

            LineSegment intersectLineSegment = Intersection.LineToLine(new Tekla.Structures.Geometry3d.Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), new Tekla.Structures.Geometry3d.Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point));
            Point intersectionMidPoint = Line.MidPoint(intersectLineSegment.StartPoint, intersectLineSegment.EndPoint);
            double d1 = Distance.PointToPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point),
                d2 = Distance.PointToPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point);
            Point p1, p2;
            if (d1 > d2)
            {
                if (Distance.PointToPoint(intersectionMidPoint, part1_centerLine[0] as Point) > Distance.PointToPoint(intersectionMidPoint, part1_centerLine[1] as Point))
                {
                    p1 = TeklaPH.Line.FindPointOnLine(part1_centerLine[0] as Point, part1_centerLine[1] as Point, d1 - d2);
                }
                else
                    p1 = TeklaPH.Line.FindPointOnLine(part1_centerLine[1] as Point, part1_centerLine[0] as Point, d1 - d2);

                if (Distance.PointToPoint(intersectionMidPoint, part2_centerLine[0] as Point) > Distance.PointToPoint(intersectionMidPoint, part2_centerLine[1] as Point))
                    p2 = part2_centerLine[0] as Point;
                else
                    p2 = part2_centerLine[1] as Point;


            }
            else
            {
                if (Distance.PointToPoint(intersectionMidPoint, part2_centerLine[0] as Point) > Distance.PointToPoint(intersectionMidPoint, part2_centerLine[1] as Point))
                {
                    p1 = TeklaPH.Line.FindPointOnLine(part2_centerLine[0] as Point, part2_centerLine[1] as Point, d2 - d1);
                }
                else
                    p1 = TeklaPH.Line.FindPointOnLine(part2_centerLine[1] as Point, part2_centerLine[0] as Point, d2 - d1);

                if (Distance.PointToPoint(intersectionMidPoint, part1_centerLine[0] as Point) > Distance.PointToPoint(intersectionMidPoint, part1_centerLine[1] as Point))
                    p2 = part1_centerLine[0] as Point;
                else
                    p2 = part1_centerLine[1] as Point;

            }
            GeometricPlane newplain = GeoPlane.CreatePlaneFromThreePoints(intersectionMidPoint, p1, p2);
            Point mid = Line.MidPoint(p1, p2);
            Point point3 = mid + newplain.GetNormal() * 50;
            GeometricPlane fittingPlain = GeoPlane.CreatePlaneFromThreePoints(intersectionMidPoint, mid, point3);
            Tekla.Structures.Model.Fitting fitting = new Tekla.Structures.Model.Fitting();

            Vector vector = newplain.GetNormal();


            fitting.Plane.Origin = intersectionMidPoint;


            fitting.Plane.AxisX = new Tekla.Structures.Geometry3d.Line(mid, intersectionMidPoint).Direction;
            fitting.Plane.AxisY = new Tekla.Structures.Geometry3d.Line(mid, point3).Direction;
            fitting.Father = part1;
            fitting.Insert();

            Tekla.Structures.Model.Fitting fitting1 = new Tekla.Structures.Model.Fitting();




            fitting1.Plane.Origin = intersectionMidPoint;


            fitting1.Plane.AxisX = new Tekla.Structures.Geometry3d.Line(mid, intersectionMidPoint).Direction;
            fitting1.Plane.AxisY = new Tekla.Structures.Geometry3d.Line(mid, point3).Direction;
            fitting1.Father = part2;
            fitting1.Insert();
        }
        public static void PartFitting(Part part1, Part part2, double gap)//only works if connection that have to be made at flange or web
        {
            Beam beam1 = part1 as Beam;
            Beam beam2 = part2 as Beam;
            ArrayList centerLine1 = beam1.GetCenterLine(false), centerLine2 = beam2.GetCenterLine(false);
            List<Face_> beam1_faces = Faces.Get_faces(beam1, true);
            Point mid = Line.MidPoint(centerLine2[0] as Point, centerLine2[1] as Point);
            double distance = 0, hold = 0;
            Face_ beam1_inter_face = null;

            foreach (Face_ face in beam1_faces)
            {
                GeometricPlane geometricPlane = ConvertFaceToGeometricPlane(face.Face);
                LineSegment lineSegment = new LineSegment(centerLine2[0] as Point, centerLine2[1] as Point);
                Point intersectingPoint = Intersection.LineSegmentToPlane(lineSegment, geometricPlane);
                if (intersectingPoint != null)
                {
                    distance = Distance.PointToPoint(intersectingPoint, mid);
                    if ((distance < hold || hold == 0.0) && !new List<int> { 1, 3, 7, 9, 0, 10, 6, 4 }.Contains(beam1_faces.IndexOf(face)))
                    {
                        hold = distance;
                        beam1_inter_face = face;

                    }
                }

            }
            Tekla.Structures.Model.Fitting fitting = new Tekla.Structures.Model.Fitting();
            fitting.Plane = new Plane();
            fitting.Plane = new Plane();
            Vector vector = beam1_inter_face.Vector;
            Point point = Faces.Get_Points(beam1_inter_face.Face)[0] as Point;

            Point point1 = new Point(point.X + gap * vector.X, point.Y + gap * vector.Y, point.Z + gap * vector.Z);
            fitting.Plane.Origin = point1;
            Faces.GetFaceAxes(beam1_inter_face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();

        }
    }
    public class DisplayPrompt
    {
        public bool Display_Prompt(string s)
        {
            return Operation.DisplayPrompt(s);

        }
    }
    public class SurfaceFinder
    {
        public static List<GeometricPlane> GetFlangeOutterSurfacePlane(Part part1)
        {
            List<Face_> face_s = Faces.Get_faces(part1, true);
            ArrayList centerPoints = part1.GetCenterLine(false);
            TSG.Line centerLine = new TSG.Line(centerPoints[0] as Point, centerPoints[1] as Point);
            Point mid = Line.MidPoint(centerPoints[0] as Point, centerPoints[1] as Point);
            face_s.RemoveAt(face_s.Count - 1);
            face_s.RemoveAt(face_s.Count - 1);
            List<Face_> cp_faces = face_s.OrderByDescending(fa => Distance.PointToPoint(mid, Projection.PointToPlane(mid, ConvertFaceToGeometricPlane(fa.Face)))).ToList();
            return new List<GeometricPlane> { ConvertFaceToGeometricPlane(cp_faces[0].Face), ConvertFaceToGeometricPlane(cp_faces[1].Face) };
        }
        public static double getFlangeDistance(Part part1)
        {
            List<GeometricPlane> geometricPlanes = GetFlangeOutterSurfacePlane(part1);
            Point point = new Point(0, 0, 0),
                p1 = Projection.PointToPlane(point, geometricPlanes[0]),
                p2 = Projection.PointToPlane(point, geometricPlanes[1]);
            return Distance.PointToPoint(p1, p2);
        }
        public static double WebThickness(Part part)
        {
            double webThickness = 0;
            part.GetReportProperty("PROFILE.WEB_THICKNESS", ref webThickness);
            double width = webThickness;
            return width;
        }
    }
}
