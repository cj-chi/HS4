using System.Collections.Generic;
using AIChara;
using HarmonyLib;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

namespace HS2OrbitAndExciter
{
    /// <summary>
    /// Helpers for orbit: get chaFemales from HScene, six-focus bone names, set focus/distance/clothes.
    /// </summary>
    public static class OrbitHelpers
    {
        public const string BoneHead = "cf_J_Head";
        public const string BoneChest = "cf_J_Mune00";
        public const string BonePelvis = "cf_J_Kokan";

        public static ChaControl[] GetChaFemales(HScene hScene)
        {
            if (hScene == null) return null;
            var t = Traverse.Create(hScene);
            var arr = t.Field("chaFemales").GetValue();
            return arr as ChaControl[];
        }

        /// <summary>Get world position of a bone on female (0 or 1). Returns null if not found.</summary>
        public static Vector3? GetBonePosition(ChaControl[] chaFemales, int femaleIndex, string boneName)
        {
            if (chaFemales == null || femaleIndex < 0 || femaleIndex >= chaFemales.Length) return null;
            var cha = chaFemales[femaleIndex];
            if (cha == null || cha.objBodyBone == null) return null;
            var tr = cha.objBodyBone.transform.FindLoop(boneName);
            if (tr == null) return null;
            return tr.position;
        }

        /// <summary>Six focus indices: 0=Head, 1=Chest, 2=Pelvis (female0), 3=Head2, 4=Chest2, 5=Pelvis2 (female1).</summary>
        public static Vector3? GetFocusPosition(ChaControl[] chaFemales, int focusIndex, Transform transBase)
        {
            if (chaFemales == null || transBase == null) return null;
            string bone;
            int femaleIdx;
            switch (focusIndex)
            {
                case 0: bone = BoneHead; femaleIdx = 0; break;
                case 1: bone = BoneChest; femaleIdx = 0; break;
                case 2: bone = BonePelvis; femaleIdx = 0; break;
                case 3: bone = BoneHead; femaleIdx = 1; break;
                case 4: bone = BoneChest; femaleIdx = 1; break;
                case 5: bone = BonePelvis; femaleIdx = 1; break;
                default: return null;
            }
            var worldPos = GetBonePosition(chaFemales, femaleIdx, bone);
            if (!worldPos.HasValue) return null;
            return transBase.InverseTransformPoint(worldPos.Value);
        }

        /// <summary>Max focus count: 6 if two females, else 3.</summary>
        public static int GetMaxFocusIndex(ChaControl[] chaFemales)
        {
            if (chaFemales == null || chaFemales.Length == 0) return 0;
            if (chaFemales.Length > 1 && chaFemales[1] != null && chaFemales[1].objBodyBone != null)
                return 6;
            return 3;
        }

        /// <summary>Sequence 0,1,2,3,2,1 (index 0..5). Returns stage 0..3 for given index.</summary>
        public static int ClothesSequenceStage(int index)
        {
            int[] seq = { 0, 1, 2, 3, 2, 1 };
            return seq[((index % 6) + 6) % 6];
        }

        /// <summary>Infer current clothes stage 0..3 from first character; return sequence index (0..5) so next step is from current state.</summary>
        public static int GetClothesSequenceIndexFromCurrent(ChaControl[] chaFemales)
        {
            int stage = GetCurrentClothesStage(chaFemales);
            return stage;
        }

        /// <summary>0=Full, 1=Half, 2=KeepAccessories, 3=FullOff. Sampled from first female via reflection (game API may vary).</summary>
        private static int GetCurrentClothesStage(ChaControl[] chaFemales)
        {
            if (chaFemales == null || chaFemales.Length == 0) return 0;
            var c = chaFemales[0];
            if (c == null) return 0;
            var t = Traverse.Create(c);
            var getState = t.Method("GetClothesState", new[] { typeof(int) });
            if (getState == null || !getState.MethodExists()) return 0;
            object r0 = getState.GetValue(0);
            int s0 = r0 is int i0 ? i0 : 0;
            if (s0 == 0) return 0;
            if (s0 == 1) return 1;
            object r4 = getState.GetValue(4);
            int s4 = r4 is int i4 ? i4 : 0;
            return s4 == 0 ? 2 : 3;
        }

        /// <summary>Clothes stage 0=Full, 1=Half, 2=KeepAccessories, 3=FullOff. Apply to all characters in HScene.</summary>
        public static void SetClothesStage(ChaControl[] chaFemales, ChaControl[] chaMales, int stage)
        {
            void ApplyTo(ChaControl c)
            {
                if (c == null) return;
                switch (stage)
                {
                    case 0:
                        c.SetClothesStateAll(0);
                        break;
                    case 1:
                        c.SetClothesState(0, 1); c.SetClothesState(1, 1);
                        c.SetClothesState(2, 1); c.SetClothesState(3, 1);
                        break;
                    case 2:
                        c.SetClothesState(0, 2); c.SetClothesState(1, 2);
                        c.SetClothesState(2, 2); c.SetClothesState(3, 2);
                        c.SetClothesState(4, 0); c.SetClothesState(5, 0);
                        c.SetClothesState(6, 0); c.SetClothesState(7, 0);
                        break;
                    case 3:
                        c.SetClothesStateAll(2);
                        break;
                }
            }
            if (chaFemales != null)
                foreach (var c in chaFemales) ApplyTo(c);
            if (chaMales != null)
                foreach (var c in chaMales) ApplyTo(c);
        }

        public static ChaControl[] GetChaMales(HScene hScene)
        {
            if (hScene == null) return null;
            var t = Traverse.Create(hScene);
            var arr = t.Field("chaMales").GetValue();
            return arr as ChaControl[];
        }

        /// <summary>Collect all AnimationListInfo from lstAnimInfo (all categories).</summary>
        public static List<HScene.AnimationListInfo> GetAllPoseList()
        {
            var tables = HSceneManager.HResourceTables;
            if (tables?.lstAnimInfo == null) return new List<HScene.AnimationListInfo>();
            var list = new List<HScene.AnimationListInfo>();
            for (int i = 0; i < tables.lstAnimInfo.Length; i++)
            {
                if (tables.lstAnimInfo[i] == null) continue;
                list.AddRange(tables.lstAnimInfo[i]);
            }
            return list;
        }

        /// <summary>Pick a random pose different from current (exclude current, no repeat).</summary>
        public static HScene.AnimationListInfo PickNextPose(HScene.AnimationListInfo current, List<HScene.AnimationListInfo> all)
        {
            if (all == null || all.Count == 0) return null;
            var exclude = new List<HScene.AnimationListInfo>(all);
            if (current != null)
                exclude.RemoveAll(x => x == current);
            if (exclude.Count == 0) return null;
            return exclude[Random.Range(0, exclude.Count)];
        }
    }
}
