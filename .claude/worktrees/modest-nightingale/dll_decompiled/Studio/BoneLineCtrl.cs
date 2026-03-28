using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Studio;

public class BoneLineCtrl : MonoBehaviour
{
	[SerializeField]
	private Material material;

	private void Draw(OCIChar _oCIChar)
	{
		if (_oCIChar.charInfo.visibleAll && _oCIChar.oiCharInfo.enableFK)
		{
			List<OCIChar.BoneInfo> listBones = _oCIChar.listBones;
			if (_oCIChar.oiCharInfo.activeFK[0])
			{
				DrawLine(listBones, 100, 102, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 100, 104, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 106, 108, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 114, 116, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 200, 201, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 201, 290, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 290, 291, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 202, 204, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 204, 206, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 208, 210, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 210, 212, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 214, 216, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 216, 218, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 292, 293, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 293, 294, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 294, 295, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 296, 297, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 297, 298, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 298, 299, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 220, 221, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 221, 222, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 222, 223, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 223, 224, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 225, 227, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 227, 229, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 229, 231, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 231, 233, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 233, 235, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 237, 239, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 239, 241, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 243, 245, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 245, 247, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 249, 251, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 251, 253, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 255, 256, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 256, 257, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 257, 258, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 258, 259, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 260, 262, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 262, 264, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 264, 266, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 266, 268, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 268, 270, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 272, 274, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 274, 276, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 278, 280, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 280, 282, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 284, 286, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 286, 288, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 350, 352, Studio.optionSystem.colorFKHair);
				DrawLine(listBones, 354, 356, Studio.optionSystem.colorFKHair);
			}
			if (_oCIChar.oiCharInfo.activeFK[1])
			{
				Draw(listBones, 1, 1, Studio.optionSystem.colorFKNeck);
			}
			if (_oCIChar.oiCharInfo.activeFK[2])
			{
				Draw(listBones, 53, 4, Studio.optionSystem.colorFKBreast);
				Draw(listBones, 59, 4, Studio.optionSystem.colorFKBreast);
			}
			if (_oCIChar.oiCharInfo.activeFK[3])
			{
				Draw(listBones, 3, 2, Studio.optionSystem.colorFKBody);
				DrawLine(listBones, 5, 6, Studio.optionSystem.colorFKBody);
				Draw(listBones, 6, 3, Studio.optionSystem.colorFKBody);
				DrawLine(listBones, 5, 10, Studio.optionSystem.colorFKBody);
				Draw(listBones, 10, 3, Studio.optionSystem.colorFKBody);
				DrawLine(listBones, 3, 14, Studio.optionSystem.colorFKBody);
				Draw(listBones, 14, 3, Studio.optionSystem.colorFKBody);
				DrawLine(listBones, 3, 18, Studio.optionSystem.colorFKBody);
				Draw(listBones, 18, 3, Studio.optionSystem.colorFKBody);
				DrawLine(listBones, 65, 66, Studio.optionSystem.colorFKBody);
			}
			if (_oCIChar.oiCharInfo.activeFK[4])
			{
				Draw(listBones, 22, 2, Studio.optionSystem.colorFKRightHand);
				Draw(listBones, 25, 2, Studio.optionSystem.colorFKRightHand);
				Draw(listBones, 28, 2, Studio.optionSystem.colorFKRightHand);
				Draw(listBones, 31, 2, Studio.optionSystem.colorFKRightHand);
				Draw(listBones, 34, 2, Studio.optionSystem.colorFKRightHand);
			}
			if (_oCIChar.oiCharInfo.activeFK[5])
			{
				Draw(listBones, 37, 2, Studio.optionSystem.colorFKLeftHand);
				Draw(listBones, 40, 2, Studio.optionSystem.colorFKLeftHand);
				Draw(listBones, 43, 2, Studio.optionSystem.colorFKLeftHand);
				Draw(listBones, 46, 2, Studio.optionSystem.colorFKLeftHand);
				Draw(listBones, 49, 2, Studio.optionSystem.colorFKLeftHand);
			}
			if (_oCIChar.oiCharInfo.activeFK[6])
			{
				int num = 400;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
				num += 6;
				Draw(listBones, num, 5, Studio.optionSystem.colorFKSkirt);
			}
		}
	}

	private void Draw(List<OCIChar.BoneInfo> _bones, int _start, int _num, Color _color)
	{
		for (int i = 0; i < _num; i++)
		{
			DrawLine(_bones, _start + i, _start + i + 1, _color);
		}
	}

	private void DrawLine(List<OCIChar.BoneInfo> _bones, int _start, int _end, Color _color)
	{
		OCIChar.BoneInfo boneInfo = _bones.Find((OCIChar.BoneInfo v) => v.boneID == _start);
		if (boneInfo != null && boneInfo.boneWeight)
		{
			OCIChar.BoneInfo boneInfo2 = _bones.Find((OCIChar.BoneInfo v) => v.boneID == _end);
			if (boneInfo2 != null && boneInfo2.boneWeight)
			{
				DrawLine(boneInfo.posision, boneInfo2.posision, _color);
			}
		}
	}

	private void DrawLine(Vector3 _s, Vector3 _e, Color _color)
	{
		GL.Color(_color);
		GL.Vertex(_s);
		GL.Vertex(_e);
	}

	private void OnPostRender()
	{
		if (Studio.optionSystem == null || !Studio.optionSystem.lineFK)
		{
			return;
		}
		IEnumerable<OCIChar> enumerable = from v in Studio.GetSelectObjectCtrl()
			where v.kind == 0
			select v as OCIChar;
		if (enumerable == null || enumerable.Count() == 0)
		{
			return;
		}
		material.SetPass(0);
		GL.PushMatrix();
		GL.Begin(1);
		foreach (OCIChar item in enumerable)
		{
			Draw(item);
		}
		GL.End();
		GL.PopMatrix();
	}
}
