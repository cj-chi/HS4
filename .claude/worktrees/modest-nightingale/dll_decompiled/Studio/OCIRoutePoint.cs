using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace Studio;

public class OCIRoutePoint : ObjectCtrlInfo
{
	public class PointAidInfo
	{
		private GameObject m_GameObject;

		public GuideObject guideObject { get; private set; }

		public OIRoutePointAidInfo aidInfo { get; private set; }

		public GameObject gameObject
		{
			get
			{
				if (m_GameObject == null)
				{
					m_GameObject = guideObject.gameObject;
				}
				return m_GameObject;
			}
		}

		public Transform target => guideObject.transformTarget;

		public Vector3 position => gameObject.transform.position;

		public Transform transform => gameObject.transform;

		public bool active
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
			set
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value);
				}
			}
		}

		public int layer
		{
			set
			{
				guideObject.SetLayer(gameObject, value);
			}
		}

		public PointAidInfo(GuideObject _guideObject, OIRoutePointAidInfo _aidInfo)
		{
			guideObject = _guideObject;
			aidInfo = _aidInfo;
		}
	}

	public GameObject objectItem;

	public PointAidInfo pointAidInfo;

	private VectorLine _line;

	private int segments = 160;

	public OIRoutePointInfo routePointInfo => objectInfo as OIRoutePointInfo;

	public OCIRoute route { get; private set; }

	public RoutePointComponent routePoint { get; private set; }

	public string name => routePointInfo.name;

	public int number
	{
		get
		{
			int result = -1;
			if (!int.TryParse(routePointInfo.name.Replace("ポイント", ""), out result))
			{
				return 0;
			}
			return result;
		}
		set
		{
			routePointInfo.number = value;
			routePoint.textName = ((value == 0) ? "S" : value.ToString());
			treeNodeObject.textName = name;
		}
	}

	public Vector3 position => objectItem.transform.position;

	public Transform[] transform => new Transform[2] { objectItem.transform, pointAidInfo.target };

	public List<Vector3> positions
	{
		get
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(position);
			if (connection == OIRoutePointInfo.Connection.Curve)
			{
				list.Add(routePoint.objAid.transform.position);
			}
			return list;
		}
	}

	public float speed
	{
		get
		{
			return routePointInfo.speed;
		}
		set
		{
			routePointInfo.speed = value;
		}
	}

	public StudioTween.EaseType easeType
	{
		get
		{
			return routePointInfo.easeType;
		}
		set
		{
			routePointInfo.easeType = value;
		}
	}

	public OIRoutePointInfo.Connection connection
	{
		get
		{
			return routePointInfo.connection;
		}
		set
		{
			routePointInfo.connection = value;
			switch (value)
			{
			case OIRoutePointInfo.Connection.Line:
				pointAidInfo.active = false;
				break;
			case OIRoutePointInfo.Connection.Curve:
				InitAidPos();
				pointAidInfo.active = true;
				break;
			}
		}
	}

	public bool link
	{
		get
		{
			return routePointInfo.link;
		}
		set
		{
			routePointInfo.link = value;
		}
	}

	public bool isLink
	{
		get
		{
			if (routePointInfo.link)
			{
				return routePointInfo.connection == OIRoutePointInfo.Connection.Curve;
			}
			return false;
		}
	}

	public bool isParentDelete { get; set; }

	public bool visible
	{
		set
		{
			routePoint.visible = value;
			lineActive = value;
		}
	}

	public VectorLine line => _line;

	public bool lineActive
	{
		set
		{
			if (_line != null)
			{
				_line.active = value;
			}
		}
	}

	public override ObjectCtrlInfo this[int _idx]
	{
		get
		{
			if (_idx != 0)
			{
				return route;
			}
			return this;
		}
	}

	public OCIRoutePoint(OCIRoute _route, OIRoutePointInfo _info, GameObject _obj, GuideObject _guide, TreeNodeObject _treeNode)
	{
		route = _route;
		objectInfo = _info;
		objectItem = _obj;
		guideObject = _guide;
		treeNodeObject = _treeNode;
		routePoint = _obj.GetComponent<RoutePointComponent>();
		isParentDelete = false;
		_line = null;
	}

	public override void OnDelete()
	{
		if (!isParentDelete)
		{
			route.DeletePoint(this);
		}
		if (_line != null)
		{
			VectorLine.Destroy(ref _line);
		}
		Singleton<GuideObjectManager>.Instance.Delete(guideObject);
		Singleton<GuideObjectManager>.Instance.Delete(pointAidInfo.guideObject);
		Object.Destroy(objectItem);
		Studio.DeleteInfo(objectInfo);
	}

	public override void OnAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
	}

	public override void OnLoadAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
	}

	public override void OnDetach()
	{
	}

	public override void OnSelect(bool _select)
	{
		int layer = LayerMask.NameToLayer(_select ? "Studio/Col" : "Studio/Select");
		pointAidInfo.layer = layer;
	}

	public override void OnDetachChild(ObjectCtrlInfo _child)
	{
	}

	public override void OnSavePreprocessing()
	{
		base.OnSavePreprocessing();
	}

	public override void OnVisible(bool _visible)
	{
		routePoint.visible = _visible && route.visibleLine;
	}

	public void UpdateLine(Vector3 _pos)
	{
		List<Vector3> list = positions;
		list.Add(_pos);
		if (_line == null)
		{
			switch (connection)
			{
			case OIRoutePointInfo.Connection.Line:
				_line = new VectorLine("Line", list, Studio.optionSystem.routeLineWidth, LineType.Continuous);
				break;
			case OIRoutePointInfo.Connection.Curve:
				_line = new VectorLine("Spline", new List<Vector3>(segments + 1), Studio.optionSystem.routeLineWidth, LineType.Continuous);
				_line.MakeSpline(list.ToArray(), segments, loop: false);
				break;
			}
			_line.joins = Joins.Weld;
			_line.color = route.routeInfo.color;
			_line.continuousTexture = false;
			_line.Draw3DAuto();
			_line.layer = LayerMask.NameToLayer("Studio/Camera");
			_line.active = route.routeInfo.visibleLine;
			return;
		}
		switch (connection)
		{
		case OIRoutePointInfo.Connection.Line:
		{
			List<Vector3> points = _line.points3;
			for (int i = 0; i < list.Count; i++)
			{
				points[i] = list[i];
			}
			_line.points3 = points;
			break;
		}
		case OIRoutePointInfo.Connection.Curve:
			_line.MakeSpline(list.ToArray(), segments, loop: false);
			break;
		}
		_line.lineWidth = Studio.optionSystem.routeLineWidth;
		_line.Draw3DAuto();
		_line.active = route.routeInfo.visibleLine;
	}

	public void DeleteLine()
	{
		if (_line != null)
		{
			VectorLine.Destroy(ref _line);
		}
	}

	private void InitAidPos()
	{
		if (!pointAidInfo.aidInfo.isInit)
		{
			DeleteLine();
			int num = route.listPoint.IndexOf(this);
			Vector3 b = ((num + 1 >= route.listPoint.Count) ? route.listPoint[0].position : route.listPoint[num + 1].position);
			Vector3 vector = Vector3.Lerp(position, b, 0.5f);
			pointAidInfo.aidInfo.changeAmount.pos = objectItem.transform.InverseTransformPoint(vector);
			pointAidInfo.aidInfo.isInit = true;
		}
	}

	public void SetEnable(bool _value, bool _first = false)
	{
		guideObject.SetEnable(_first ? (-1) : (_value ? 1 : 0), (!_first) ? (-1) : (_value ? 1 : 0));
		pointAidInfo.guideObject.SetEnable(_value ? 1 : 0);
	}
}
