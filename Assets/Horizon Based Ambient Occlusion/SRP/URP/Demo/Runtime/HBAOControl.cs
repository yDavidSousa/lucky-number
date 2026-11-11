using UnityEngine;
using UnityEngine.Rendering;

namespace HorizonBasedAmbientOcclusion.Universal
{
    public class HBAOControl : MonoBehaviour
    {
        public VolumeProfile postProcessProfile;
        public UnityEngine.UI.Slider aoRadiusSlider;

        private bool m_HbaoDisplayed = true;

        public void Start()
        {
            HBAO hbao;
            postProcessProfile.TryGet(out hbao);

            if (hbao != null)
            {
                hbao.SetAoIntensity(1);
                hbao.SetDebugMode(HBAO.DebugMode.Disabled);
                hbao.SetAoRadius(aoRadiusSlider.value);
            }
        }

        public void ToggleHBAO()
        {
            HBAO hbao;
            postProcessProfile.TryGet(out hbao);

            if (hbao != null)
            {
                m_HbaoDisplayed = !m_HbaoDisplayed;
                hbao.SetAoIntensity(m_HbaoDisplayed ? 1 : 0);
            }
        }

        public void ToggleShowAO()
        {
            HBAO hbao;
            postProcessProfile.TryGet(out hbao);

            if (hbao != null)
            {
                if (hbao.GetDebugMode() != HBAO.DebugMode.Disabled)
                    hbao.SetDebugMode(HBAO.DebugMode.Disabled);
                else
                    hbao.SetDebugMode(HBAO.DebugMode.AOOnly);
            }
        }

        public void UpdateAoRadius()
        {
            HBAO hbao;
            postProcessProfile.TryGet(out hbao);

            if (hbao != null)
                hbao.SetAoRadius(aoRadiusSlider.value);
        }
    }
}