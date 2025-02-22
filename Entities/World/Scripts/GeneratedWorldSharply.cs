using Godot;
using Godot.Collections;
using Test.Entities.Helpers;

[Tool]
public partial class GeneratedWorldSharply : Node3D
{
	// Impromptu buttons
	[Export]
	public bool Clear
	{
		get => false;
		set
		{
			if (Engine.IsEditorHint())
			{
				GD.Print("Cleared the sumbitch");
				_gridMap = GetNode<GridMap>("GridMap");
				_gridMap.Clear();
			}
		}
	}

	[Export]
	public bool Start
	{
		get => false;
		set
		{
			if (Engine.IsEditorHint())
			{
				_gridMap = GetNode<GridMap>("GridMap");
				Generate();
			}
		}
	}

	// Actual Properties
	[Export(PropertyHint.Range, "2, 1000, 1")] public int PointsInside = 5;
	[Export(PropertyHint.Range, "1, 10, 1")] public int PointsEdge = 2;
	[Export(PropertyHint.Range, "1, 1000, 1")] public int RoadWidth = 20;
	[Export(PropertyHint.Range, "2, 1000, 1")] public int BoxSize = 300;
	[Export(PropertyHint.Range, "0, 500, 1")] public int MarginOffsetEdge = 20;
	[Export(PropertyHint.Range, "0, 500, 1")] public int MarginOffsetInside = 5;
	[Export(PropertyHint.Range, "1, 1000, 1")] public int PavementWidth = 5;

	// these aren't implemented
	[Export(PropertyHint.Range, "1, 1000, 1")] public int RoadLentgth = 5;
	[Export(PropertyHint.Range, "1, 1000, 1")] public int BuildingDensity = 5;

	private GridMap _gridMap;
	private int _sizedOffsetted;

	private Array<Vector3> _rpp3d = new();
	private Array<Vector2> _rpp2d = new();

	public override void _Ready()
	{
		_gridMap = GetNode<GridMap>("GridMap");
		// GD.Print("Startuem Ready");
		Generate();
	}

	private void Generate()
	{
		_sizedOffsetted = BoxSize - (MarginOffsetInside * 2);
		_gridMap.Clear();

		if (Engine.IsEditorHint())
		{
			GD.Print("Startuem");
			_rpp2d.Clear();
			_rpp3d.Clear();

			MakeBorder();
		}
		// GD.Print("Startuem Generate");

		MakeBase();
		MakePoints();
		var mst = FindMst();
		ConnectRoadPoints(mst);
	}

	public AStar2D FindMst()
	{
		// GD.Print("Startuem Mst");
		var delGraph = new AStar2D();
		var mstGraph = new AStar2D();

		// GD.Print("Rpp", _rpp2d);

		foreach (var point in _rpp2d)
		{
			delGraph.AddPoint(delGraph.GetAvailablePointId(), point);
			mstGraph.AddPoint(mstGraph.GetAvailablePointId(), point);
		}

		var _rpp2dFloat = new Vector2[_rpp2d.Count];

		for (var i = 0; i < _rpp2d.Count; i++)
		{
			_rpp2dFloat[i] = _rpp2d[i];
		}

		var delanuay = Geometry2D.TriangulateDelaunay(_rpp2dFloat);
		var delanuay2 = delanuay.ToGDArray();


		foreach (var idx in GD.Range(delanuay2.Count / 3))
		{
			var p1 = delanuay2[0];
			var p2 = delanuay2[1];
			var p3 = delanuay2[2];
			delanuay2.Reverse();
			delanuay2.Resize(delanuay2.Count - 3);
			delanuay2.Reverse();

			// GD.Print($"p1: {p1}, p2: {p2}, p3: {p3}, sCurr: {delanuay2.Count / 3} sPrev: {(delanuay2.Count / 3) + 3}");

			delGraph.ConnectPoints(p1, p2);
			delGraph.ConnectPoints(p2, p3);
			delGraph.ConnectPoints(p1, p3);

		}

		// while (delanuay2.Count / 3 != 0)
		// {
		// 		var p1 = delanuay2[0];
		// 		var p2 = delanuay2[1];
		// 		var p3 = delanuay2[2];
		// 		delanuay2.Reverse();
		// 		delanuay2.Resize(delanuay2.Count - 3);
		// 		delanuay2.Reverse();
		//
		// 		// GD.Print($"p1: {p1}, p2: {p2}, p3: {p3}, sCurr: {delanuay2.Count / 3} sPrev: {(delanuay2.Count / 3) + 3}");
		//
		// 		delGraph.ConnectPoints(p1, p2);
		// 		delGraph.ConnectPoints(p2, p3);
		// 		delGraph.ConnectPoints(p1, p3);
		//
		// 		// GD.Print(delanuay2.Count / 3);
		// }

		var visitedPoints = new Array<int>();

		visitedPoints.Add((int)(GD.Randi() % _rpp3d.Count));

		while (visitedPoints.Count <= mstGraph.GetPointCount())
		{
			// GD.Print($"Initial {visitedPoints.Count}, {mstGraph.GetPointCount()}");
			var possCons = new Array<Array<int>>();

			foreach (var point in visitedPoints)
			{
				foreach (int connection in delGraph.GetPointConnections(point))
				{
					var newCon = new Array<int>();
					if (!visitedPoints.Contains(connection))
					{
						newCon.Add(point);
						newCon.Add(connection);

						possCons.Add(newCon);
					}
				}
			}

			if (possCons.Count == 0) break;

			if (possCons != null)
			{
				var conn = possCons.PickRandom();

				foreach (var possCon in possCons)
				{
					if (_rpp2d[possCon[0]].DistanceSquaredTo(_rpp2d[possCon[1]]) <
							_rpp2d[conn[0]].DistanceSquaredTo(_rpp2d[conn[1]]))
						conn = possCon;

				}
				visitedPoints.Add(conn[1]);
				mstGraph.ConnectPoints(conn[0], conn[1]);
				delGraph.DisconnectPoints(conn[0], conn[1]);
			}
		}
		return mstGraph;
	}

	private void ConnectRoadPoints(AStar2D mstGraph)
	{
		var roads = new Array<Array<Vector3>>();

		foreach (var point in mstGraph.GetPointIds())
		{
			foreach (var connection in mstGraph.GetPointConnections(point))
			{
				if (connection > point)
				{
					var pointFrom = _rpp3d[(int)point];
					var pointTo = _rpp3d[(int)connection];

					var road = new Array<Vector3>();
					road.Add(pointFrom);
					road.Add(pointTo);

					roads.Add(road);
				}
			}
		}

		var aStarGrid = new AStarGrid2D();
		aStarGrid.Region = new Rect2I(0, 0, BoxSize, BoxSize);
		aStarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		aStarGrid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
		aStarGrid.Update();

		foreach (var road in roads)
		{
			var pointFrom = new Vector2I((int)road[0].X, (int)road[0].Z);
			var pointTo = new Vector2I((int)road[1].X, (int)road[1].Z);

			var roadPath = aStarGrid.GetPointPath(pointFrom, pointTo);

			foreach (var point in roadPath)
			{
				var pointPos = new Vector3I((int)point.X, 0, (int)point.Y);

				for (var i = 0; i < RoadWidth + PavementWidth; i++)
				{
					for (var j = 0; j < RoadWidth + PavementWidth; j++)
					{
						var pXpZp = new Vector3I(pointPos.X + i, pointPos.Y, pointPos.Z + j);
						var pXpZm = new Vector3I(pointPos.X + i, pointPos.Y, pointPos.Z - j);
						var pXmZp = new Vector3I(pointPos.X - i, pointPos.Y, pointPos.Z + j);
						var pXmZm = new Vector3I(pointPos.X - i, pointPos.Y, pointPos.Z - j);

						_gridMap.SetCellItem(pXpZp, 1);
						_gridMap.SetCellItem(pXpZm, 1);
						_gridMap.SetCellItem(pXmZp, 1);
						_gridMap.SetCellItem(pXmZm, 1);
					}
				}
			}
		}


		foreach (var road in roads)
		{
			var pointFrom = new Vector2I((int)road[0].X, (int)road[0].Z);
			var pointTo = new Vector2I((int)road[1].X, (int)road[1].Z);

			var roadPath = aStarGrid.GetPointPath(pointFrom, pointTo);


			foreach (var point in roadPath)
			{
				var pointPos = new Vector3I((int)point.X, 0, (int)point.Y);

				for (var i = 0; i < RoadWidth; i++)
				{
					for (var j = 0; j < RoadWidth; j++)
					{
						var pXpZp = new Vector3I(pointPos.X + i, pointPos.Y, pointPos.Z + j);
						var pXpZm = new Vector3I(pointPos.X + i, pointPos.Y, pointPos.Z - j);
						var pXmZp = new Vector3I(pointPos.X - i, pointPos.Y, pointPos.Z + j);
						var pXmZm = new Vector3I(pointPos.X - i, pointPos.Y, pointPos.Z - j);

						_gridMap.SetCellItem(pXpZp, 0);
						_gridMap.SetCellItem(pXpZm, 0);
						_gridMap.SetCellItem(pXmZp, 0);
						_gridMap.SetCellItem(pXmZm, 0);
					}
				}
			}
		}

		foreach (var road in roads)
		{
			var pointFrom = new Vector2I((int)road[0].X, (int)road[0].Z);
			var pointTo = new Vector2I((int)road[1].X, (int)road[1].Z);

			var roadPath = aStarGrid.GetPointPath(pointFrom, pointTo);
			foreach (var point in roadPath)
			{
				var pointPos = new Vector3I((int)point.X, 0, (int)point.Y);
				_gridMap.SetCellItem(pointPos, 5);
			}
		}
	}

	private void MakePoints()
	{
		var pointPosition = new Vector3I();
		var placedPointsInside = 0;
		var placedPointsEdge = 0;
		var placedDirection = new[] { false, false, false, false };

		while (placedPointsInside < PointsInside)
		{
			pointPosition = new Vector3I((int)(GD.Randi() % _sizedOffsetted + MarginOffsetInside),
					0,
					(int)(GD.Randi() % _sizedOffsetted + MarginOffsetInside));
			if (!IsOnMapEdge(pointPosition.X, pointPosition.Z))
			{
				_gridMap.SetCellItem(pointPosition, 0);
				placedPointsInside++;
			}
		}

		while (placedPointsEdge < PointsEdge)
		{
			pointPosition = new Vector3I(
					(int)GD.Randi() % BoxSize, 0, (int)GD.Randi() % BoxSize);
			if (IsOnMapEdge(pointPosition.X, pointPosition.Z) &&
					IsAloneOnMapEdge(pointPosition, placedDirection))
			{
				_gridMap.SetCellItem(pointPosition, 0);
				placedPointsEdge++;
			}
		}

		foreach (var point in _gridMap.GetUsedCellsByItem(0))
		{
			_rpp3d.Add(point);
		}

		foreach (var point in _rpp3d)
		{
			_rpp2d.Add(new Vector2(point.X, point.Z));
		}
	}

	private bool IsAloneOnMapEdge(Vector3I pointPosition, bool[] placedDirection)
	{
		for (var i = 0; i < BoxSize; i++)
		{
			if (i == pointPosition.Z && pointPosition.X == 0 &&
					!placedDirection[0])
			{
				placedDirection[0] = true;
				return true;
			}
			else if (i == pointPosition.X && pointPosition.Z == 0 &&
					!placedDirection[1])
			{
				placedDirection[1] = true;
				return true;
			}

			else if (i == pointPosition.Z && pointPosition.X == BoxSize - 1 &&
					!placedDirection[2])
			{
				placedDirection[2] = true;
				return true;
			}

			else if (i == pointPosition.X && pointPosition.Z == BoxSize - 1 &&
					!placedDirection[3])
			{
				placedDirection[3] = true;
				return true;
			}
		}
		return false;
	}

	private void MakeBase()
	{
		for (var i = 0; i < BoxSize; i++)
		{
			for (var j = 0; j < BoxSize; j++)
			{
				if (!IsOnMapEdge(i, j)) _gridMap.SetCellItem(new Vector3I(i, 0, j), 6);
			}
		}
	}

	private void MakeBorder()
	{
		for (var i = 0; i < BoxSize; i++)
		{
			for (var j = 0; j < BoxSize; j++)
			{
				if (IsOnMapEdge(i, j)) _gridMap.SetCellItem(new Vector3I(i, 0, j), 5);
			}
		}
	}

	private bool IsOnMapEdge(int x, int y)
	{
		return (x == 0 || y == 0 || x == BoxSize - 1 || y == BoxSize - 1);
	}
}
