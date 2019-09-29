using UnityEngine;

namespace cylvester
{
    interface IRectangularSelection
    {
        ref Rect Selection { get; }

        void Start(Vector2 mousePosition);
        void Update(Vector2 mousePosition);
    }
    
    public class RectangularSelection : IRectangularSelection
    {
        private readonly Rect paintSpace_;

        private Rect selectedArea_;
        private Rect selectionRect_;
        private readonly int textureWidth_;
        private readonly int textureHeight_;

        public ref Rect Selection => ref selectionRect_;
        
        public RectangularSelection(ref Rect paintSpace, int textureWidth, int textureHeight)
        {
            paintSpace_ = paintSpace;
            textureWidth_ = textureWidth;
            textureHeight_ = textureHeight;
        }

        public void Start(Vector2 mousePosition)
        {
            selectedArea_.x = mousePosition.x;
            selectedArea_.y = mousePosition.y;
        }

        public void Update(Vector2 mousePosition)
        {
            selectedArea_.width = mousePosition.x - selectedArea_.x;
            selectedArea_.height = mousePosition.y - selectedArea_.y;
            var xPos = (selectedArea_.x - paintSpace_.x) / paintSpace_.width;
            var yPos = (selectedArea_.y - paintSpace_.y) / paintSpace_.height;
            var width = selectedArea_.width / paintSpace_.width;
            var height = selectedArea_.height / paintSpace_.height;

            selectionRect_.x = xPos * textureWidth_;
            selectionRect_.y = yPos * textureHeight_;
            selectionRect_.width = width * textureWidth_;
            selectionRect_.height = height * textureHeight_;
        }
    }
}