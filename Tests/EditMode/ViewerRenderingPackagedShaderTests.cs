using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace Deucarian.ViewerRendering.Tests.EditMode
{
    public sealed class ViewerRenderingPackagedShaderTests
    {
        [Test]
        public void PackagedVolumeEffectsSurvivePlayerShaderStripping()
        {
            ViewerRenderingSettings settings =
                ViewerRenderingSettings.LoadReferencePreset();
            Assert.That(AssetDatabase.GetAssetPath(settings.RealisticProfile),
                Does.StartWith("Packages/"));
            Assert.That(settings.RealisticProfile.TryGet(out Bloom bloom), Is.True);
            Assert.That(bloom.IsActive(), Is.True);

            using (var serialized = new SerializedObject(settings.GlobalSettings))
            {
                SerializedProperty entries = serialized.FindProperty(
                    "m_Settings.m_SettingsList.m_List");
                Assert.That(entries, Is.Not.Null);
                URPShaderStrippingSetting stripping = null;
                for (int index = 0; index < entries.arraySize; index++)
                {
                    if (entries.GetArrayElementAtIndex(index).managedReferenceValue
                        is URPShaderStrippingSetting candidate)
                    {
                        stripping = candidate;
                        break;
                    }
                }

                Assert.That(stripping, Is.Not.Null);
                // URP's volume discovery only scans Assets/, ignoring our profiles.
                Assert.That(stripping.stripUnusedPostProcessingVariants, Is.False,
                    "Package-owned Bloom and other runtime volume effects must survive builds.");
                Assert.That(stripping.stripUnusedVariants, Is.True,
                    "General unused shader variants should still be stripped.");
            }
        }
    }
}
