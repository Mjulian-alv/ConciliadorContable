// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Recorridos reales de navegador, archivos y aislamiento.
const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_PATH || 'playwright');
const fs = require('node:fs');
(async () => {
 const browser = await chromium.launch({ executablePath: process.env.EDGE_PATH, headless: true });
 const errors = [];
 const base = process.env.ARCA_WEB_URL || 'http://127.0.0.1:5084';
 const out = '.superpowers/sdd/2026-10-05-arca-web'; fs.mkdirSync(out,{recursive:true});
 async function login(context) {
   const page = await context.newPage(); page.on('pageerror',e=>errors.push(e.message));
   await page.goto(base+'/account/login');
   await page.getByRole('button',{name:'Entrar a la demostración'}).click();
   // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - El login aterriza en el menú principal; ARCA Comprobantes está en /arca/comprobantes.
   await page.getByRole('heading',{name:'Conciliador Contable',exact:true}).waitFor(); await page.goto(base+'/arca/comprobantes');
   await page.getByRole('heading',{name:'Conciliación',exact:true}).waitFor(); return page;
 }
 const context=await browser.newContext({viewport:{width:1440,height:1000}});
 const p=await login(context);
 await p.locator('select').first().selectOption({label:'Proveedores · pendientes de pago'});
 await p.getByLabel('CUIT del proveedor').fill('30-12345678-1');
 await p.getByRole('button',{name:'Continuar →'}).click();
 await p.getByRole('button',{name:'Cargar ejemplo de Proveedor'}).click();
 await p.getByRole('heading',{name:'Tipos de comprobante',exact:true}).waitFor();
 for(const [raw,target] of [['FA','001'],['NCA','003'],['NDA','002'],['REC','EXCLUIR'],['PAGO','EXCLUIR']]){
   const select=p.getByLabel('Equivalencia '+raw,{exact:true});
   const row=select.locator('xpath=ancestor::tr');
   const wasSaved=(await row.getByRole('button').innerText())==='Guardado' && (await select.inputValue())===target; await select.selectOption(target); const confirm=row.getByRole('button',{name:'Confirmar',exact:true});
   if(!wasSaved){await confirm.click();await row.getByRole('button',{name:'Guardado',exact:true}).waitFor();}
 }
 await p.getByLabel('El archivo contiene los comprobantes pendientes de pago.').check();
 await p.getByRole('button',{name:'Conciliar comprobantes →'}).click();
 await p.locator('#resultado').waitFor();
 assert.deepEqual(await p.locator('.stat strong').allTextContents(),['4','1','1','1']);
 const download=p.waitForEvent('download');
 await p.getByRole('button',{name:'Descargar Excel'}).click();
 const xlsx=await download;assert.equal(xlsx.suggestedFilename(),'conciliacion-proveedor.xlsx');await xlsx.saveAs(out+'/resultado-proveedor.xlsx');
 await p.getByRole('button',{name:'Resumen por CUIT'}).click(); await p.getByText('-14.900,00',{exact:true}).waitFor();
 assert.match(await p.locator('#resultado table').innerText(),/-14.900,00/);
 await p.locator('#resultado').scrollIntoViewIfNeeded();await p.screenshot({path:out+'/proveedor.png'});
 console.log('OK proveedor: equivalencias, exclusiones, cuatro estados, resumen y Excel');
 const c2=await browser.newContext({viewport:{width:390,height:844}});const second=await login(c2);
 assert.equal(await second.locator('#resultado').count(),0);assert.equal(await second.locator('input[type=file]').count(),0);
 await second.screenshot({path:out+'/mobile.png',fullPage:true});assert(await second.evaluate(()=>document.documentElement.scrollWidth<=window.innerWidth));
 console.log('OK sesión independiente y pantalla móvil sin desborde');
 const invalid=Buffer.from('Tipo;Fecha;Numero;Importe\n'+Array(201).fill('FA;05/09/2026;5-1234;100,00').join('\n')+'\nFA;31/02/2026;5-9999;100,00');
 await p.locator('.upload-grid input[type=file]').first().setInputFiles({name:'invalido.csv',mimeType:'text/csv',buffer:invalid});
 await p.getByText('Revisar archivo',{exact:true}).first().waitFor();
 assert.equal(await p.locator('#resultado').count(),0);
 assert(await p.getByRole('button',{name:'Conciliar comprobantes →'}).isDisabled());
 console.log('OK fila inválida al final bloquea y descarta el resultado');
 await p.getByRole('button',{name:'Cambiar perfil / CUIT'}).click();
 await p.locator('select').first().selectOption({label:'ARCA · comprobantes recibidos'});
 await p.getByRole('button',{name:'Continuar →'}).click();
 await p.getByRole('button',{name:'Cargar ejemplo de ARCA'}).click();
 await p.getByRole('button',{name:'Conciliar comprobantes →'}).click();await p.locator('#resultado').waitFor();
 assert.deepEqual(await p.locator('.stat strong').allTextContents(),['4','1','1','1']);
 await p.getByRole('link',{name:'Administrar perfiles'}).click();
 await p.getByRole('heading',{name:'Perfiles de conciliación'}).waitFor();
 await p.getByRole('button',{name:/ARCA · comprobantes recibidos/}).click();
 await p.locator('input[type=file]').setInputFiles({name:'cuits.csv',mimeType:'text/csv',buffer:Buffer.from('CUIT\n30-12345678-1\n30123456781')});
 await p.getByText('1 CUIT válidos y únicos',{exact:true}).waitFor();
 await p.getByRole('button',{name:'Reemplazar lista guardada'}).click();
 await p.getByLabel('Filtrar ambos archivos por lista de CUIT').check();
 await p.getByText('Cambios guardados automáticamente',{exact:false}).waitFor();
 await p.screenshot({path:out+'/perfiles.png',fullPage:true});
 await p.getByRole('link',{name:'Ir a conciliar'}).click();
 // El perfil cambió: se debe seleccionar de nuevo y no se exporta una conciliación vieja.
 assert.equal(await p.locator('#resultado').count(),0);
 console.log('OK ARCA y lista CUIT validada y deduplicada');
 await context.close();await c2.close();await browser.close();
 assert.deepEqual(errors,[]);console.log('Navegador: 5 recorridos correctos, sin errores de página.');
})().catch(e=>{console.error(e);process.exit(1);});
