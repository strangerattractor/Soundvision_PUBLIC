using UnityEngine;

namespace cylvester
{
    interface IRectangularSelection
    {
        (Rect, bool) Update(Vector2 mousePosition, ref Rect paintSpace);

        // void Pressed(Vector2 mousePosition, ref Rect paintSpace);
        // void Dragged(Vector2 mousePosition, ref Rect paintSpace);
        // void Released(Vector2 mousePosition, ref Rect paintSpace);
    }
    
    public class RectangularSelection : IRectangularSelection
    {
        private readonly Rect paintSpace_;

        private Rect selectedArea_;
        private Rect lastSelectionRect = new Rect(0, 0, 0, 0);
        private readonly int textureWidth_;
        private readonly int textureHeight_;
        private bool dragging = false;
        
        public RectangularSelection(int textureWidth, int textureHeight)
        {
            textureWidth_ = textureWidth;
            textureHeight_ = textureHeight;
        }

        public (Rect, bool) Update(Vector2 mousePosition, ref Rect paintSpace)
        {
            if (!Event.current.isMouse || Event.current.button != 0) return (lastSelectionRect, false);

            bool updated = false;
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                {
                    Pressed(Event.current.mousePosition, ref paintSpace);
                    break;
                }

                case EventType.MouseDrag:
                {
                    Dragged(Event.current.mousePosition, ref paintSpace);
                    updated = true;
                    break;
                }

                case EventType.MouseUp:
                {
                    Released(Event.current.mousePosition, ref paintSpace);
                    break;
                }
            }
            return (lastSelectionRect, updated);
        }

        public void Pressed(Vector2 mousePosition, ref Rect paintSpace)
        {
            float x = mousePosition.x - paintSpace.x;
            float y = mousePosition.y - paintSpace.y;
            
            float edgeThreshold = 30;
            if (x < -edgeThreshold) {
                return;
            }
            if (y < -edgeThreshold) {
                return;
            }
            if (x >= paintSpace.width + edgeThreshold) {
                return;
            }
            if (y >= paintSpace.height + edgeThreshold) {
                return;
            }

            if (x < 0) x = 0;
            if (y < 0) y = 0;
            selectedArea_.x = x;
            selectedArea_.y = y;

            dragging = true;
        }

        public void Dragged(Vector2 mousePosition, ref Rect paintSpace)
        {
            if (dragging == false) {
                return;
            }

            float x = mousePosition.x - paintSpace.x;
            float y = mousePosition.y - paintSpace.y;
            selectedArea_.width = x - selectedArea_.x;
            selectedArea_.height = y - selectedArea_.y;
            var xNormalized = selectedArea_.x / paintSpace.width;
            var yNormalized = selectedArea_.y / paintSpace.height;
            var wNormalized = selectedArea_.width / paintSpace.width;
            var hNormalized = selectedArea_.height / paintSpace.height;

            if (wNormalized < 0) {
                wNormalized = -wNormalized;
                xNormalized = xNormalized - wNormalized;
                if (xNormalized < 0) {
                    wNormalized += xNormalized;
                    xNormalized = 0;
                }
            }
            if (hNormalized < 0) {
                hNormalized = -hNormalized;
                yNormalized = yNormalized - hNormalized;
                if (yNormalized < 0) {
                    hNormalized += yNormalized;
                    yNormalized = 0;
                }
            }
            if (xNormalized + wNormalized > 1) {
                wNormalized = 1 - xNormalized;
            }
            if (yNormalized + hNormalized > 1) {
                hNormalized = 1 - yNormalized;
            }

            lastSelectionRect = new Rect
            {
                x = xNormalized * textureWidth_,
                y = yNormalized * textureHeight_,
                width = wNormalized * textureWidth_,
                height = hNormalized * textureHeight_
            };
        }

        public void Released(Vector2 mousePosition, ref Rect paintSpace)
        {
            dragging = false;
        }
    }
}