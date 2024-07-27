@tool
extends Node3D


# Impromtu buttons
@export var clear: bool = false:
	set(value):
		if Engine.is_editor_hint():
			print("cleared the sumbitch")
			grid_map.clear()

@export var start: bool = false:
	set(value):
		if Engine.is_editor_hint():
			generate()

# Actual properties
@export var points: int = 5
@export var road_width: int = 3
@export var box_size: int = 20
@export var pavement_width: int = 6
@export var road_length: int = 5

@onready var grid_map: GridMap = $"GridMap"


var _rpp3d: PackedVector3Array = []
var _rpp2d: PackedVector2Array = []


func _ready() -> void:
	grid_map.clear()
	if not Engine.is_editor_hint(): generate()
	pass


func generate() -> void:
	grid_map.clear()
	if Engine.is_editor_hint():
		print("Startuem")
		_rpp3d = []
		_rpp2d = []
		make_border()

	make_base()
	make_points()

	var del_graph := AStar2D.new()
	var mst_graph := AStar2D.new()


	for point: Vector2i in _rpp2d:
		del_graph.add_point(del_graph.get_available_point_id(), point)
		mst_graph.add_point(mst_graph.get_available_point_id(), point)

	var delanuay := Array(Geometry2D.triangulate_delaunay(_rpp2d))
	
	for i in delanuay.size()/3:
		var p1: int = delanuay.pop_front()
		var p2: int = delanuay.pop_front()
		var p3: int = delanuay.pop_front()
		
		del_graph.connect_points(p1, p2)
		del_graph.connect_points(p2, p3)
		del_graph.connect_points(p1, p3)


	var visited_points: PackedInt32Array = []

	visited_points.append(randi() % _rpp3d.size())
	var iterator: int = 0
	# print(mst_graph.get_point_capacity())
	# print("rpp3d", _rpp3d)
	# print("rpp2d", _rpp2d)

	while visited_points.size() != mst_graph.get_point_count():
		# print("while iteration")
		var possible_connections: Array[PackedInt32Array] = []
		
		for point in visited_points:
			# print("visited_points iteration")
			for connection in del_graph.get_point_connections(point):
				# print("del_graph iteration")
				if not visited_points.has(connection):
					var new_connection: PackedInt32Array = [point, connection]
					possible_connections.append(new_connection)
					# print("pc: ", possible_connections)

		if possible_connections: 
			var conn: PackedInt32Array = possible_connections.pick_random()
			# print("conn: ", conn)
			for pc in possible_connections:
				# print("possible_connections iteration")
				if _rpp2d[pc[0]].distance_squared_to(_rpp2d[pc[1]]) < \
				_rpp2d[conn[0]].distance_squared_to(_rpp2d[conn[1]]):
					conn = pc
					# print("conn(pc): ", conn)
			visited_points.append(conn[1])
			# print("visited_points", visited_points)
			mst_graph.connect_points(conn[0], conn[1])
			# print(mst_graph.get_point_count())
			del_graph.disconnect_points(conn[0], conn[1])
			# print(del_graph.get_point_count())

		# print("finished iteration", iterator)
		if iterator == 200: break

		connect_points(mst_graph)



func make_base() -> void:
	for i in box_size:
		for j in box_size:
			if not ( i == 0 or j == 0 or i == box_size - 1 or j == box_size - 1 ):
				grid_map.set_cell_item(Vector3i(i, 0, j), 3)


func make_points() -> void:
	var point_position: Vector3i
	var placed_points: int = 0

	for i in box_size:
		for j in box_size:
			if (i != 0 or j != 0 or i != box_size - 1 or j != box_size - 1) and  placed_points <= points:
				point_position = Vector3i(randi() % (box_size - 1), 0 , randi() % (box_size - 1))
				grid_map.set_cell_item(point_position, 2)
				placed_points += 1

	_rpp3d = grid_map.get_used_cells_by_item(2)
	for point in _rpp3d:
		_rpp2d.append(Vector2i(point.x, point.z))


func connect_points(mst_graph: AStar2D) -> void:
	var roads: Array[PackedVector3Array] = []

	for point in mst_graph.get_point_ids():
		for connection in mst_graph.get_point_connections(point):
			if connection > point:
				var point_from: Vector3i = _rpp3d[point]
				var point_to: Vector3i = _rpp3d[connection]

				var road: PackedVector3Array = [point_from, point_to]
				roads.append(road)

	var a_star_grid := AStarGrid2D.new()
	a_star_grid.region = Rect2i(0, 0, box_size, box_size)

	a_star_grid.update()
	a_star_grid.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_NEVER
	a_star_grid.default_estimate_heuristic = AStarGrid2D.HEURISTIC_MANHATTAN

	for road in roads:
		var point_from := Vector2i(road[0].x, road[0].z)
		var point_to := Vector2i(road[1].x, road[1].z)
		var road_path: PackedVector2Array = a_star_grid.get_point_path(point_from, point_to)

		for point in road_path:
			var point_position := Vector3i(point.x, 0, point.y)
			if grid_map.get_cell_item(point_position) == 3 or grid_map.get_cell_item(point_position) == -1:
				for i in road_width:
					for j in road_width:
						grid_map.set_cell_item(Vector3i(point_position.x - i, point_position.y, point_position.z + j), 2)
						grid_map.set_cell_item(Vector3i(point_position.x + i, point_position.y, point_position.z - j), 2)

				# for width in road_width:
				# 	grid_map.set_cell_item(Vector3i(point_position.x - width, point_position.y, point_position.z + width), 2)
				# 	grid_map.set_cell_item(Vector3i(point_position.x - width, point_position.y, point_position.z), 2)
				# 	grid_map.set_cell_item(Vector3i(point_position.x - width, point_position.y, point_position.z - width), 2)
				#
				# 	grid_map.set_cell_item(Vector3i(point_position.x, point_position.y, point_position.z + width), 2)
				# 	grid_map.set_cell_item(point_position, 2)
				# 	grid_map.set_cell_item(Vector3i(point_position.x, point_position.y, point_position.z - width), 2)
				#
				# 	grid_map.set_cell_item(Vector3i(point_position.x + width, point_position.y, point_position.z + width), 2)
				# 	grid_map.set_cell_item(Vector3i(point_position.x + width, point_position.y, point_position.z), 2)
				# 	grid_map.set_cell_item(Vector3i(point_position.x + width, point_position.y, point_position.z - width), 2)


func add_pavement() -> void:
	pass


func add_buildings() -> void:
	pass
				

func make_border() -> void:
	for i in box_size:
		for j in box_size:
			if i == 0 or j == 0 or i == box_size - 1 or j == box_size - 1:
				
				grid_map.set_cell_item(Vector3i(i, 0, j), 5)
