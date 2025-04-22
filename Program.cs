using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Ruta del archivo JSON
string archivo = "tareas.json";

// Funciones
List<Tarea> CargarTareas()
{
    if (!File.Exists(archivo)) File.WriteAllText(archivo, "[]");
    var json = File.ReadAllText(archivo);
    return JsonSerializer.Deserialize<List<Tarea>>(json) ?? [];
}

void GuardarTareas(List<Tarea> tareas)
{
    var json = JsonSerializer.Serialize(tareas, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(archivo, json);
}

// Endpoints

// HOME
app.MapGet("/", () => "SERVIDOR FUNCIONANDO - API DE TAREAS");

// GET: todas las tareas
app.MapGet("/tareas", () =>
{
    var tareas = CargarTareas();
    return Results.Json(tareas, statusCode: 200);
});

// GET: buscar por ID
app.MapGet("/tareas/{id:int}", (int id) =>
{
    var tareas = CargarTareas();
    var tarea = tareas.FirstOrDefault(t => t.Id == id);
    return tarea is not null
        ? Results.Json(tarea)
        : Results.NotFound($"No se encontró tarea con ID: {id}");
});

// POST: agregar tarea
app.MapPost("/tareas", (Tarea nueva) =>
{
    if (string.IsNullOrWhiteSpace(nueva.Titulo))
        return Results.BadRequest(new { error = "El título es obligatorio." }); // Código 400

    var tareas = CargarTareas();
    nueva.Id = tareas.Any() ? tareas.Max(t => t.Id) + 1 : 1;
    tareas.Add(nueva);
    GuardarTareas(tareas);

    return Results.Created($"/tareas/{nueva.Id}", nueva); // Código 201
});

// DELETE: eliminar por ID
app.MapDelete("/tareas/{id:int}", (int id) =>
{
    var tareas = CargarTareas();
    var tarea = tareas.FirstOrDefault(t => t.Id == id);
    if (tarea is null)
        return Results.NotFound($"No se encontró tarea con ID: {id}");

    tareas.Remove(tarea);
    GuardarTareas(tareas);
    return Results.Ok($"Tarea con ID {id} eliminada correctamente");
});

// PUT: modificar tarea
app.MapPut("/tareas/{id:int}", (int id, Tarea modificada) =>
{
    var tareas = CargarTareas();
    var index = tareas.FindIndex(t => t.Id == id);
    if (index == -1)
        return Results.NotFound($"No se encontró tarea con ID: {id}");

    modificada.Id = id;
    tareas[index] = modificada;
    GuardarTareas(tareas);
    return Results.Ok($"Tarea con ID {id} actualizada");
});

app.Run();
