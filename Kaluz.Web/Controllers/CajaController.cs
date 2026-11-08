using System.Security.Claims;using Kaluz.Web.Services;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Authorization;
namespace Kaluz.Web.Controllers;
[Authorize(Roles="Administrador,Vendedora")]public class CajaController(Db db,KaluzService svc):Controller{
 int U=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 public IActionResult Index(){ViewBag.Caja=db.List("SELECT TOP 1 * FROM Caja WHERE IdUsuario=@u ORDER BY IdCaja DESC",Db.P("@u",U)).FirstOrDefault();ViewBag.Movimientos=db.List("SELECT TOP 100 mc.*,mp.Nombre AS Metodo FROM MovimientoCaja mc JOIN Caja c ON c.IdCaja=mc.IdCaja LEFT JOIN MetodoPago mp ON mp.IdMetodoPago=mc.IdMetodoPago WHERE c.IdUsuario=@u ORDER BY mc.IdMovimientoCaja DESC",Db.P("@u",U));
 ViewBag.Resumen=db.List("SELECT mp.Nombre Metodo, SUM(CASE WHEN mc.Tipo='Ingreso' THEN mc.Monto ELSE -mc.Monto END) Neto FROM MovimientoCaja mc JOIN MetodoPago mp ON mp.IdMetodoPago=mc.IdMetodoPago WHERE mc.IdCaja=(SELECT TOP 1 IdCaja FROM Caja WHERE IdUsuario=@u AND Estado='Abierta' ORDER BY IdCaja DESC) GROUP BY mp.Nombre",Db.P("@u",U));return View();}
 [HttpPost][ValidateAntiForgeryToken]public IActionResult Abrir(decimal inicial){try{svc.OpenCash(U,inicial);TempData["ok"]="Caja abierta.";}catch(Exception e){TempData["err"]=e.Message;}return RedirectToAction("Index");}
 [HttpPost][ValidateAntiForgeryToken]public IActionResult Movimiento(string tipo,string concepto,decimal monto){try{svc.CashMovement(U,tipo,concepto,monto);TempData["ok"]="Movimiento guardado.";}catch(Exception e){TempData["err"]=e.Message;}return RedirectToAction("Index");}
 [HttpPost][ValidateAntiForgeryToken]public IActionResult Cerrar(decimal contado){try{svc.CloseCash(U,contado);TempData["ok"]="Caja cerrada; diferencia calculada.";}catch(Exception e){TempData["err"]=e.Message;}return RedirectToAction("Index");}
}
