using System.Security.Claims;using Kaluz.Web.Models;using Kaluz.Web.Services;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Authorization;
namespace Kaluz.Web.Controllers;
[Authorize]public class InventarioController(Db db,KaluzService svc):Controller{
 public IActionResult Index(){ViewBag.Items=db.List("SELECT v.IdVariante,p.Nombre,v.Talla,v.Color,v.SKU,v.StockActual,v.StockMinimo FROM ProductoVariante v JOIN Producto p ON p.IdProducto=v.IdProducto ORDER BY p.Nombre");ViewBag.History=db.List("SELECT TOP 60 m.Fecha,p.Nombre,v.SKU,m.Tipo,m.Cantidad,m.StockResultante,m.Motivo FROM MovimientoInventario m JOIN ProductoVariante v ON v.IdVariante=m.IdVariante JOIN Producto p ON p.IdProducto=v.IdProducto ORDER BY m.IdMovimiento DESC");return View();}
 [HttpPost][Authorize(Roles="Administrador")][ValidateAntiForgeryToken]public IActionResult Ajustar(StockForm form){try{svc.Adjust(form,int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));TempData["ok"]="Stock ajustado.";}catch(Exception ex){TempData["err"]=ex.Message;}return RedirectToAction("Index");}
}
