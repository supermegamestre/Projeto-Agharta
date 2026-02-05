using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SmallUtilities
{
    public class HitboxController
    {
        private PolygonCollider2D mainCollider;
        private List<PolygonCollider2D> referenceColliders;
        private int index = 0;

        private Dictionary<string, PolygonCollider2D> hitboxDictionary;
        private string stringIndex;

        private enum hitboxControllerType
        {
            index,
            dictionary
        }

        private hitboxControllerType type;

        public HitboxController(PolygonCollider2D mainCollider, IEnumerable<PolygonCollider2D> referenceColliders)
        {
            this.mainCollider = mainCollider;
            this.mainCollider.pathCount = 0;
            this.referenceColliders = referenceColliders.ToList();
            type = hitboxControllerType.index;
        }

        public HitboxController(PolygonCollider2D mainCollider, Dictionary<string, PolygonCollider2D> hitboxDictionary)
        {
            this.mainCollider = mainCollider;
            this.mainCollider.pathCount = 0;
            this.hitboxDictionary = hitboxDictionary;
            type = hitboxControllerType.dictionary;
        }

        public HitboxController(IEnumerable<PolygonCollider2D> referenceColliders) 
        { 
            this.referenceColliders = referenceColliders.ToList(); 
            type = hitboxControllerType.index;
        }

        public HitboxController(Dictionary<string, PolygonCollider2D> hitboxDictionary)
        {
            this.hitboxDictionary = hitboxDictionary;
            type = hitboxControllerType.dictionary;
        }

        public void setHitbox (PolygonCollider2D collider, int index)
        {
            if (type != hitboxControllerType.index) throw new Exception("this isn't a index type controller you idiot!");
            if(collider == null) throw new ArgumentNullException("which collider?" + nameof(collider));
            if(index < 0 || index > referenceColliders.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index != this.index)
            {
                collider.SetPath(0, referenceColliders[index].GetPath(0));
                this.index = index;
            }
        }

        public void setHitbox(PolygonCollider2D collider, string name)
        {
            if (type != hitboxControllerType.dictionary) throw new Exception("this isn't a dictionary type controller you idiot!");
            if (collider == null)throw new ArgumentNullException("which collider?" + nameof(collider));
            if (!hitboxDictionary.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));
            if (name != stringIndex)
            {
                collider.SetPath(0, hitboxDictionary[name].GetPath(0));
                stringIndex = name;
            }
        }

        public void setHitbox(int index)
        {
            if (type != hitboxControllerType.index) throw new Exception("this isn't a index type controller you idiot!");
            if (mainCollider == null) throw new ArgumentNullException("you didn't create this controller with a main hitbox you idiot!" + nameof(mainCollider));
            if (index < 0 || index > referenceColliders.Count) throw new ArgumentOutOfRangeException(nameof(index));
            
            mainCollider.SetPath(0, referenceColliders[index].GetPath(0));
            this.index = index;
            
        }

        public void setHitbox(string name)
        {
            if (type != hitboxControllerType.dictionary) throw new Exception("this isn't a dictionary type controller you idiot!");
            if (mainCollider == null) throw new ArgumentNullException("you didn't create this controller with a main hitbox you idiot!" + nameof(mainCollider));
            if (!hitboxDictionary.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));
            
            mainCollider.SetPath(0, hitboxDictionary[name].GetPath(0));
            stringIndex = name;
            
        }

        public void clearHitbox() => mainCollider.pathCount = 0;

        public void clearHitbox(PolygonCollider2D collider) => collider.pathCount = 0;
    }
}