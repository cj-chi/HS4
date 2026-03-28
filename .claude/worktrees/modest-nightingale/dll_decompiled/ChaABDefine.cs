public static class ChaABDefine
{
	public const string MainManifest = "abdata";

	public const string RandomNameListAssetBundle = "list/characustom/namelist.unity3d";

	public const string RandomNameListAsset = "RandNameList_Name";

	public const string EtcAssetBundle = "chara/etc.unity3d";

	public const string BaseObjectAssetBundle = "chara/oo_base.unity3d";

	public const string BaseMaterialAssetBundle = "chara/mm_base.unity3d";

	public const string BodyBoneAsset = "p_cf_anim";

	public const string HeadBoneAsset = "p_cf_head_bone";

	public const string MaleBodyAsset = "p_cm_body_00";

	public const string FemaleBodyAsset = "p_cf_body_00";

	public const string FemaleBodyNormalAsset = "p_cf_body_00_Nml";

	public const string MaleBodyHitAsset = "p_cm_body_hit_low";

	public const string FemaleBodyHitAsset = "p_cf_body_00_hit_low";

	public const string BlackTex2048Asset = "black2048";

	public const string BlackTex4096Asset = "black4096";

	public const string MaleBodyMaterialAsset = "cm_m_skin_body_00";

	public const string FemaleBodyMaterialAsset = "cf_m_skin_body_00";

	public const string CreateMatFaceBaseAsset = "create_skin_face";

	public const string CreateMatFaceGlossAsset = "create_skin detail_face";

	public const string CreateMatBodyBaseAsset = "create_skin_body";

	public const string CreateMatBodyGlossAsset = "create_skin detail_body";

	public const string CreateMatClothesAsset = "create_clothes";

	public const string CreateMatClothesGlossAsset = "create_clothes detail";

	public const string ShapeListAssetBundle = "list/customshape.unity3d";

	public const string MaleShapeHeadListAsset = "cm_customhead";

	public const string FemaleShapeHeadListAsset = "cf_customhead";

	public const string FemaleShapeBodyListAsset = "cf_custombody";

	public const string FemaleShapeHandListAsset = "cf_customhand";

	public const string FemaleShapeBodyAnimeAsset = "cf_anmShapeBody";

	public const string FemaleShapeHandAnimeAsset = "cf_anmShapeHand";

	public const string MaleSilhouetteAsset = "p_cm_body_silhouette";

	public const string FemaleSilhouetteAsset = "p_cf_body_silhouette";

	public const string MalePresetAssetBundle = "custom/00/presets_m_00.unity3d";

	public const string FemalePresetAssetBundle = "custom/00/presets_f_00.unity3d";

	public const string MalePresetAsset = "cm_mannequin";

	public const string FemalePresetAsset = "cf_mannequin";

	public const string MaleCustomAnimAssetBundle = "custom/00/anim_m_00.unity3d";

	public const string FemaleCustomAnimAssetBundle = "custom/00/anim_f_00.unity3d";

	public const string MaleCustomAnimAsset = "edit_M";

	public const string FemaleCustomAnimAsset = "edit_F";

	public const string HairShaderMatAssetBundle = "chara/hair_shader_mat.unity3d";

	public const string HairShaderMatDitheringAsset = "hair_dithering";

	public const string HairShaderMatCutoutAsset = "hair_cutout";

	public const string ExpressionListAssetBundle = "list/expression.unity3d";

	public const string MaleExpressionListAsset = "cm_expression";

	public const string FemaleExpressionListAsset = "cf_expression";

	public static string BodyAsset(int sex)
	{
		if (sex != 0)
		{
			return "p_cf_body_00";
		}
		return "p_cm_body_00";
	}

	public static string BodyMaterialAsset(int sex)
	{
		if (sex != 0)
		{
			return "cf_m_skin_body_00";
		}
		return "cm_m_skin_body_00";
	}

	public static string ShapeHeadListAsset(int sex)
	{
		if (sex != 0)
		{
			return "cf_customhead";
		}
		return "cm_customhead";
	}

	public static string SilhouetteAsset(int sex)
	{
		if (sex != 0)
		{
			return "p_cf_body_silhouette";
		}
		return "p_cm_body_silhouette";
	}

	public static string PresetAssetBundle(int sex)
	{
		if (sex != 0)
		{
			return "custom/00/presets_f_00.unity3d";
		}
		return "custom/00/presets_m_00.unity3d";
	}

	public static string PresetAsset(int sex)
	{
		if (sex != 0)
		{
			return "cf_mannequin";
		}
		return "cm_mannequin";
	}

	public static string CustomAnimAssetBundle(int sex)
	{
		if (sex != 0)
		{
			return "custom/00/anim_f_00.unity3d";
		}
		return "custom/00/anim_m_00.unity3d";
	}

	public static string CustomAnimAsset(int sex)
	{
		if (sex != 0)
		{
			return "edit_F";
		}
		return "edit_M";
	}
}
