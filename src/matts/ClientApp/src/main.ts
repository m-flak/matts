/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import '@angular/localize/init';
import { enableProdMode, isDevMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export function websocketBaseUrl() {
  let baseUrl = getBaseUrl();
  baseUrl = baseUrl.replace(new URL(baseUrl).protocol, 'wss:');
  if (isDevMode()) {
    baseUrl = baseUrl.replace(new URL(baseUrl).port, '7017');
  }
  return baseUrl;
}

if (environment.production) {
  enableProdMode();
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'WS_BASE_URL', useFactory: websocketBaseUrl, deps: [] },
];

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
