// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 4 - Descarga privada sin guardar resultados en rutas públicas.
window.downloadFile = async (name, stream) => {
    const bytes = await stream.arrayBuffer();
    const url = URL.createObjectURL(new Blob([bytes]));
    const link = document.createElement('a'); link.href = url; link.download = name;
    link.click(); URL.revokeObjectURL(url);
};
