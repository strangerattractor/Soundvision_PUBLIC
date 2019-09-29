using UnityEngine;

namespace cylvester
{
    interface IRectangularSelection
    {
        void Start(Vector2 mousePosition);
        void Update(Vector2 mousePosition, ref Rect paintSpace, ref Rect selectionRect);
    }
    
    public class RectangularSelection : IRectangularSelection
    {
        private readonly Rect paintSpace_;

        private Rect selectedArea_;
        private readonly int textureWidth_;
        private readonly int textureHeight_;

        
        public RectangularSelection(int textureWidth, int textureHeight)
        {
            textureWidth_ = textureWidth;
            textureHeight_ = textureHeight;
        }

        public void Start(Vector2 mousePosition)
        {
            selectedArea_.x = mousePosition.x;
            selectedArea_.y = mousePosition.y;
        }

        public void Update(Vector2 mousePosition, ref Rect paintSpace, ref Rect selectionRect)
        {
            selectedArea_.width = mousePosition.x - selectedArea_.x;
            selectedArea_.height = mousePosition.y - selectedArea_.y;
            var xPos = (selectedArea_.x - paintSpace.x) / paintSpace.width;
            var yPos = (selectedArea_.y - paintSpace.y) / paintSpace.height;
            var width = selectedArea_.width / paintSpace.width;
            var height = selectedArea_.height / paintSpace.height;

            selectionRect.x = xPos * textureWidth_;
            selectionRect.y = yPos * textureHeight_;
            selectionRect.width = width * textureWidth_;
            selectionRect.height = height * textureHeight_;
        }
    }
}