using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Cargamos la configuracion externa
var puerto = builder.Configuration.GetValue<int>("Puerto");
var ambiente = builder.Configuration.GetValue<string>("Ambiente");

// Cambiamos el puerto desde configuracion
app.Urls.Add($"http://localhost:{puerto}");

// Mostramos el ambiente al iniciar
Console.WriteLine($" Ambiente actrual: {ambiente}");

// Iniciamos el meotodo para manejar tareas JSON

List<Tarea> CargarTareas()
{
    var json = File.ReadAllText("tareas.json");
    return JsonSerializer.Deserialize<List<Tarea>>(json) ?? [];
}

void GuardarTareas(List<Tarea> tareas)
{
    var json = JsonSerializer.Serialize(tareas, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText("tareas.json", json);
}

// GET: Todas las tareas
app.MapGet("/tareas", () =>
{
    var tareas = CargarTareas();
    return Results.Json(new { status = "ok", result = tareas }, statusCode: 200);
});

// Get por ID con switch

app.MapGet("/tareas/{id:int}", (int id) =>
{
    var tareas = CargarTareas();
    var tarea = tareas.FirstOrDefault(t => t.Id == id);

    return tarea switch{
        null => Results.StatusCode(404),
        _ => Results.Json(tarea)
    };
});

// POST para crear tarea 
app.MapPost("/tareas", (Tarea nuevo) => 
{
    if (string.IsNullOrWhiteSpace(nuevo.Titulo))
    return Results.Json(new { status = "error", result = "El Titulo es obligatorio." }, statusCode: 400);

    var tareas = CargarTareas();
    nuevo.Id = tareas.Any() ? tareas.Max(t => t.Id) + 1 : 1;
    tareas.Add(nuevo);
    GuardarTareas(tareas);

    return Results.Created($"/tareas/{nuevo.Id}", $" Se creo la tarea '{nuevo.Titulo}');");
});


// Delete por ID
app.MapDelete("/tareas/{id:int}", (int id) =>
{
    var tareas = CargarTareas();
    var tarea = tareas.FirstOrDefault(t => t.Id == id);

    if (tarea is null)
        return Results.Json( new { status = "error", result  = "Tarea no encontrada." }, statusCode: 404);

        tareas.Remove(tarea);
        GuardarTareas(tareas);
        return Results.Json(new { status = "ok", result = $"Tarea con ID {id} eliminada."   });
});

// PUT: Modificar tarea completa
app.MapPut("/tareas/{id:int}", (int id, Tarea tareaModificada) =>
{
    var tareas = CargarTareas();
    var index = tareas.FindIndex(t => t.Id == id);

    if (index == -1)
        return Results.Json(new { status = "error", result = "Tarea no encontrada" }, statusCode: 404);

    tareaModificada.Id = id;
    tareas[index] = tareaModificada;
    GuardarTareas(tareas);

    return Results.Ok($"Tarea {id} actualizada correctamente");
});

// PATCH: Modificar campo Completada
app.MapPatch("/tareas/{id:int}/completada", (int id, bool completada) =>
{
    var tareas = CargarTareas();
    var tarea = tareas.FirstOrDefault(t => t.Id == id);
    if (tarea is null)
        return Results.Json(new { status = "error", result = "Tarea no encontrada" }, statusCode: 404);

    tarea.Completada = completada;
    GuardarTareas(tareas);

    return Results.Ok($"Tarea {id} marcada como {(completada ? "completada" : "pendiente")}");
});

app.Run();