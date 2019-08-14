using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    //The basic canvas
    //can port different drawCmd lists to this canvas for different uses
    public class TestCanvas : RawImage
    {
        private FlushDrawer _flushDrawer;

        private FlushDrawer flushDrawer
        {
            get
            {
                if (_flushDrawer == null)
                {
                    //default material for Canvas
                    material = new Material(Shader.Find("Custom/UIDefault"));
            
                    //attach flushDraw
                    int panelWidth = (int) rectTransform.rect.width;
                    int panelHeight = (int) rectTransform.rect.height;
            
                    _flushDrawer = new TestComputeBufferMesh(panelWidth, panelHeight, 1);
                }

                return _flushDrawer;
            }
        }
        
        private void OnGUI()
        {
            if (Event.current.type.Equals(EventType.Repaint))
            {
                texture = flushDrawer.Draw();
            }
        }
    }
}