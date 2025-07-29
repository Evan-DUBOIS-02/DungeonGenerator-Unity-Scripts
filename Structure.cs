using UnityEngine;

namespace DungeonGenerator
{
    public class Structure: MonoBehaviour
    {
        [SerializeField] private GameObject ZposDoor;
        [SerializeField] private GameObject ZnegDoor;
        [SerializeField] private GameObject XposDoor;
        [SerializeField] private GameObject XnegDoor;

        private Vector2Int _roomIndex;
        public Vector2Int RoomIndex { get; set; }

        public void OpenDoor(Vector2Int direction)
        {
            if(direction == Vector2Int.left)
                XnegDoor.SetActive(false);
            else if(direction == Vector2Int.right)
                XposDoor.SetActive(false);
            else if(direction == Vector2Int.down)
                ZnegDoor.SetActive(false);
            else if(direction == Vector2Int.up)
                ZposDoor.SetActive(false);
        }
        
        public void OpenZposDoor()
        {
            
        }
        
        public void OpenZnegDoor()
        {
            
        }
        
        public void OpenXposDoor()
        {
            
        }
        
        public void OpenXnegDoor()
        {
            
        }
    }
}