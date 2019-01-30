using Microsoft.Xna.Framework;
using Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics
{
    public class Octree
    {
        public float Size { get; set; }
        public Vector3 Position { get; set; }
        public int MaxObjects { get; set; }
        public BoundingBox Bounds { get; set; }

        public List<GameObject3D> Objects { get; set; }
        public List<Octree> Nodes { get; set; }

        public Octree(float size, Vector3 position, int maxObjects)
        {
            Size = size;
            Position = position;
            MaxObjects = maxObjects;

            Objects = new List<GameObject3D>();
            Nodes = new List<Octree>();

            //TODO: create bounds of given size at given position
            float halfSize = size / 2;
            Vector3 min = new Vector3(position.X - halfSize, position.Y - halfSize, position.Z - halfSize);
            Vector3 max = new Vector3(position.X + halfSize, position.Y + halfSize, position.Z + halfSize);
            Bounds = new BoundingBox(min, max);

            DebugEngine.AddBoundingBox(Bounds, Color.LimeGreen, 1000);
            DebugEngine.AddBoundingSphere(new BoundingSphere(new Vector3(position.X, position.Y, position.Z), 1), Color.Black, 1000);
        }

        public void Subdivide()
        {
            //Sub divide the current node’s bounding box into 4 equal boxes
            float childSize = Size / 2;

            Octree FTL = new Octree(
                    childSize,
                    new Vector3(Position.X - childSize / 2, Position.Y + childSize / 2, Position.Z + childSize / 2),
                    MaxObjects);

            Octree FBL = new Octree(
                    childSize,
                    new Vector3(Position.X - childSize / 2, Position.Y - childSize / 2, Position.Z + childSize / 2),
                    MaxObjects);

            Octree FTR = new Octree(
                    childSize,
                    new Vector3(Position.X + childSize / 2, Position.Y + childSize / 2, Position.Z + childSize / 2),
                    MaxObjects);

            Octree FBR = new Octree(
                    childSize,
                    new Vector3(Position.X + childSize / 2, Position.Y - childSize / 2, Position.Z + childSize / 2),
                    MaxObjects);

            //Back

            Octree BTL = new Octree(
                    childSize,
                    new Vector3(Position.X - childSize / 2, Position.Y + childSize / 2, Position.Z - childSize / 2),
                    MaxObjects);

            Octree BBL = new Octree(
                    childSize,
                    new Vector3(Position.X - childSize / 2, Position.Y - childSize / 2, Position.Z - childSize / 2),
                    MaxObjects);

            Octree BTR = new Octree(
                    childSize,
                    new Vector3(Position.X + childSize / 2, Position.Y + childSize / 2, Position.Z - childSize / 2),
                    MaxObjects);

            Octree BBR = new Octree(
                    childSize,
                    new Vector3(Position.X + childSize / 2, Position.Y - childSize / 2, Position.Z - childSize / 2),
                    MaxObjects);

            Nodes.Add(FTL);
            Nodes.Add(FTR);
            Nodes.Add(FBL);
            Nodes.Add(FBR);
            Nodes.Add(BTL);
            Nodes.Add(BTR);
            Nodes.Add(BBL);
            Nodes.Add(BBR);
        }

        public void AddObject(GameObject3D newObject)
        {
            if (Nodes.Count == 0)
            {
                if (Objects.Count < MaxObjects)
                {
                    Objects.Add(newObject);
                }
                else
                {
                    Subdivide();

                    foreach (GameObject3D go in Objects)
                        Distribute(go);

                    Objects.Clear();
                }
            }
            else
            {
                Distribute(newObject);
            }
        }

        public void Distribute(GameObject3D newObject)
        {
            Vector3 position = newObject.World.Translation;
            foreach (Octree node in Nodes)
            {
                if (node.Bounds.Contains(position) != ContainmentType.Disjoint)
                {
                    node.AddObject(newObject);
                    break;
                }
            }
        }

        public void Process(BoundingFrustum frustum, ref List<GameObject3D> foundObjects)
        {
            if (foundObjects == null)
                foundObjects = new List<GameObject3D>();

            if (frustum.Contains(Bounds) != ContainmentType.Disjoint)
            {
                foundObjects.AddRange(Objects);

                foreach (Octree node in Nodes)
                    node.Process(frustum, ref foundObjects);
            }
        }
    }
}
