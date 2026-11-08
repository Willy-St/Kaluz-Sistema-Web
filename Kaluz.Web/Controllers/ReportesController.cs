using System.Text;
using Kaluz.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kaluz.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController(Db db) : Controller
{
    public IActionResult Index()
    {
        // Todas las secciones usan el mismo rango temporal: últimos 30 días, incluido hoy.
        const string desde = "DATEADD(day, -30, CONVERT(date, GETDATE()))";

        ViewBag.Resumen = db.List($@"
            SELECT
              (SELECT COALESCE(SUM(v.Total),0) FROM Venta v
               WHERE v.Estado='Completada' AND v.Fecha >= {desde}) AS VentasBrutas,
              (SELECT COALESCE(SUM(d.MontoReembolso),0) FROM Devolucion d
               WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}) AS Devoluciones,
              (SELECT COALESCE(SUM(v.Total),0) FROM Venta v
               WHERE v.Estado='Completada' AND v.Fecha >= {desde}) -
              (SELECT COALESCE(SUM(d.MontoReembolso),0) FROM Devolucion d
               WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}) AS VentasNetas,
              (SELECT COALESCE(SUM(dc.CantidadRecibida * dc.CostoUnitario),0)
               FROM Compra c JOIN DetalleCompra dc ON dc.IdCompra=c.IdCompra
               WHERE c.Fecha >= {desde} AND c.Estado IN ('Recibida','Parcial')) AS ComprasRecibidas,
              (SELECT COALESCE(SUM(g.Monto),0) FROM Gasto g WHERE g.Fecha >= CONVERT(date,{desde})) AS Gastos,
              (SELECT COALESCE(SUM((dv.Cantidad-COALESCE(ret.CantidadDevuelta,0))*p.PrecioCosto),0)
               FROM DetalleVenta dv
               JOIN Venta v ON v.IdVenta=dv.IdVenta
               JOIN ProductoVariante pv ON pv.IdVariante=dv.IdVariante
               JOIN Producto p ON p.IdProducto=pv.IdProducto
               OUTER APPLY (SELECT SUM(dd.Cantidad) AS CantidadDevuelta
                   FROM DetalleDevolucion dd JOIN Devolucion d ON d.IdDevolucion=dd.IdDevolucion
                   WHERE dd.IdDetalleVenta=dv.IdDetalleVenta AND d.Tipo='Devolucion'
                     AND d.Fecha >= {desde}) ret
               WHERE v.Estado='Completada' AND v.Fecha >= {desde}
                 AND dv.Cantidad >= COALESCE(ret.CantidadDevuelta,0)) AS CostoMercaderia
        ").FirstOrDefault() ?? new Dictionary<string, object?>();

        ViewBag.Utilidad = db.List($@"
            SELECT
              COALESCE((SELECT SUM(v.Total) FROM Venta v WHERE v.Estado='Completada' AND v.Fecha >= {desde}),0)
              - COALESCE((SELECT SUM(d.MontoReembolso) FROM Devolucion d WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}),0)
              - COALESCE((SELECT SUM(g.Monto) FROM Gasto g WHERE g.Fecha >= CONVERT(date,{desde})),0)
              - COALESCE((SELECT SUM((dv.Cantidad-COALESCE(ret.CantidadDevuelta,0))*p.PrecioCosto)
                    FROM DetalleVenta dv JOIN Venta v ON v.IdVenta=dv.IdVenta
                    JOIN ProductoVariante pv ON pv.IdVariante=dv.IdVariante
                    JOIN Producto p ON p.IdProducto=pv.IdProducto
                    OUTER APPLY (SELECT SUM(dd.Cantidad) AS CantidadDevuelta
                       FROM DetalleDevolucion dd JOIN Devolucion d ON d.IdDevolucion=dd.IdDevolucion
                       WHERE dd.IdDetalleVenta=dv.IdDetalleVenta AND d.Tipo='Devolucion'
                         AND d.Fecha >= {desde}) ret
                    WHERE v.Estado='Completada' AND v.Fecha >= {desde}
                      AND dv.Cantidad >= COALESCE(ret.CantidadDevuelta,0)),0) AS UtilidadEstimada
        ").FirstOrDefault() ?? new Dictionary<string, object?>();

        // Ventas brutas y reembolsos se presentan por separado por fecha de cada operación.
        ViewBag.Ventas = db.List($@"
            WITH Dias AS (
              SELECT CONVERT(date,v.Fecha) AS Dia, COUNT(*) AS Cantidad,
                     SUM(v.Total) AS Bruto
              FROM Venta v WHERE v.Estado='Completada' AND v.Fecha >= {desde}
              GROUP BY CONVERT(date,v.Fecha)
            ), Reembolsos AS (
              SELECT CONVERT(date,d.Fecha) AS Dia, SUM(d.MontoReembolso) AS Devuelto
              FROM Devolucion d WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}
              GROUP BY CONVERT(date,d.Fecha)
            )
            SELECT COALESCE(Dias.Dia,Reembolsos.Dia) AS Dia,
                   COALESCE(Dias.Cantidad,0) AS Cantidad,
                   COALESCE(Dias.Bruto,0) AS Total,
                   COALESCE(Reembolsos.Devuelto,0) AS Devoluciones,
                   COALESCE(Dias.Bruto,0)-COALESCE(Reembolsos.Devuelto,0) AS Neto
            FROM Dias FULL OUTER JOIN Reembolsos ON Reembolsos.Dia=Dias.Dia
            ORDER BY Dia DESC");

        // Agregamos devoluciones antes de unirlas para no multiplicar subtotales.
        ViewBag.Top = db.List($@"
            WITH Vendido AS (
              SELECT dv.IdDetalleVenta,p.Nombre,
                     dv.Cantidad,dv.Subtotal
              FROM DetalleVenta dv JOIN Venta v ON v.IdVenta=dv.IdVenta
              JOIN ProductoVariante pv ON pv.IdVariante=dv.IdVariante
              JOIN Producto p ON p.IdProducto=pv.IdProducto
              WHERE v.Estado='Completada' AND v.Fecha >= {desde}
            ), Devuelto AS (
              SELECT dd.IdDetalleVenta,SUM(dd.Cantidad) AS CantidadDevuelta
              FROM DetalleDevolucion dd JOIN Devolucion d ON d.IdDevolucion=dd.IdDevolucion
              WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}
              GROUP BY dd.IdDetalleVenta
            )
            SELECT TOP 10 x.Nombre,
                   SUM(x.Cantidad-COALESCE(d.CantidadDevuelta,0)) AS Unidades,
                   SUM(CASE WHEN x.Cantidad=0 THEN 0 ELSE
                     x.Subtotal * (x.Cantidad-COALESCE(d.CantidadDevuelta,0)) / x.Cantidad END) AS Importe
            FROM Vendido x LEFT JOIN Devuelto d ON d.IdDetalleVenta=x.IdDetalleVenta
            GROUP BY x.Nombre ORDER BY Unidades DESC, Importe DESC");

        ViewBag.Gastos = db.List($@"
            SELECT cg.Nombre AS Categoria,COUNT(g.IdGasto) AS Cantidad,SUM(g.Monto) AS Total
            FROM Gasto g JOIN CategoriaGasto cg ON cg.IdCategoriaGasto=g.IdCategoriaGasto
            WHERE g.Fecha >= CONVERT(date,{desde})
            GROUP BY cg.Nombre ORDER BY Total DESC");

        // El reembolso se resta al método de pago de la venta original.
        ViewBag.Pagos = db.List($@"
            WITH Cobros AS (
              SELECT v.IdMetodoPago,COUNT(*) AS Cantidad,SUM(v.Total) AS Bruto
              FROM Venta v WHERE v.Estado='Completada' AND v.Fecha >= {desde}
              GROUP BY v.IdMetodoPago
            ), Reembolsos AS (
              SELECT v.IdMetodoPago,SUM(d.MontoReembolso) AS Devuelto
              FROM Devolucion d JOIN Venta v ON v.IdVenta=d.IdVenta
              WHERE d.Tipo='Devolucion' AND d.Fecha >= {desde}
              GROUP BY v.IdMetodoPago
            )
            SELECT mp.Nombre AS Metodo,COALESCE(c.Cantidad,0) AS Cantidad,
                   COALESCE(c.Bruto,0) AS Bruto,COALESCE(r.Devuelto,0) AS Devoluciones,
                   COALESCE(c.Bruto,0)-COALESCE(r.Devuelto,0) AS Total
            FROM MetodoPago mp LEFT JOIN Cobros c ON c.IdMetodoPago=mp.IdMetodoPago
            LEFT JOIN Reembolsos r ON r.IdMetodoPago=mp.IdMetodoPago
            ORDER BY Total DESC,mp.Nombre");

        ViewBag.Compras = db.List($@"
            SELECT c.IdCompra,pr.Nombre AS Proveedor,c.Fecha,c.Estado,c.Total,
                   COALESCE(SUM(dc.CantidadRecibida*dc.CostoUnitario),0) AS TotalRecibido
            FROM Compra c JOIN Proveedor pr ON pr.IdProveedor=c.IdProveedor
            LEFT JOIN DetalleCompra dc ON dc.IdCompra=c.IdCompra
            WHERE c.Fecha >= {desde}
            GROUP BY c.IdCompra,pr.Nombre,c.Fecha,c.Estado,c.Total
            ORDER BY c.Fecha DESC");

        return View();
    }

    private static string CsvSafe(object? value)
    {
        var s = Convert.ToString(value) ?? "";
        if (s.Length > 0 && "=+-@".Contains(s[0])) s = "'" + s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    public IActionResult Csv(string tipo = "ventas")
    {
        string sql = tipo switch
        {
            "stock" => "SELECT p.Nombre,v.SKU,v.Talla,v.Color,v.StockActual,v.StockMinimo FROM ProductoVariante v JOIN Producto p ON p.IdProducto=v.IdProducto ORDER BY p.Nombre,v.Talla,v.Color",
            "gastos" => "SELECT g.Fecha,cg.Nombre AS Categoria,g.Concepto,g.Monto FROM Gasto g JOIN CategoriaGasto cg ON cg.IdCategoriaGasto=g.IdCategoriaGasto ORDER BY g.Fecha DESC",
            "compras" => "SELECT c.IdCompra,p.Nombre AS Proveedor,c.Fecha,c.NumeroDocumento,c.Total,c.Estado FROM Compra c JOIN Proveedor p ON p.IdProveedor=c.IdProveedor ORDER BY c.Fecha DESC",
            "pagos" => "SELECT mp.Nombre AS Metodo,COUNT(v.IdVenta) AS Cantidad,COALESCE(SUM(v.Total),0) AS Bruto FROM MetodoPago mp LEFT JOIN Venta v ON v.IdMetodoPago=mp.IdMetodoPago AND v.Estado='Completada' GROUP BY mp.Nombre ORDER BY mp.Nombre",
            "ventas" => "SELECT NumeroTicket,Fecha,Subtotal,Descuento,Total,Estado FROM Venta ORDER BY Fecha DESC",
            _ => "SELECT IdCliente,Nombres,Apellidos,DNI,Telefono FROM Cliente ORDER BY Nombres,Apellidos"
        };
        if (!new[] { "stock", "gastos", "compras", "pagos", "ventas", "clientes" }.Contains(tipo)) tipo = "ventas";
        var rows = db.List(sql);
        var csv = new StringBuilder();
        if (rows.Count > 0)
        {
            csv.AppendLine(string.Join(";", rows[0].Keys.Select(CsvSafe)));
            foreach (var row in rows) csv.AppendLine(string.Join(";", row.Values.Select(CsvSafe)));
        }
        return File(new UTF8Encoding(true).GetBytes(csv.ToString()), "text/csv; charset=utf-8", tipo + "_kaluz.csv");
    }
}
