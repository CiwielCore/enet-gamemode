using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.API.ParticleFx
{
    public class ParticleFxData
    {
        public float scale { get; set; } = 1f;
        public ParticleFxColor rgb { get; set; } = new ParticleFxColor();
        public float xOffset { get; set; } = 0f;
        public float yOffset { get; set; } = 0f;
        public float zOffset { get; set; } = 0f;
        public float xRot { get; set; } = 0f;
        public float yRot { get; set; } = 0f;
        public float zRot { get; set; } = 0f;

        public ParticleFxData(float scale = 1f, float xOffset = 0f, float yOffset = 0f, float zOffset = 0f, float xRot = 0f, float yRot = 0f, float zRot = 0f)
        {
            this.scale = scale;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.zOffset = zOffset;
            this.xRot = xRot;
            this.yRot = yRot;
            this.zRot = zRot;
        }
    }
}
