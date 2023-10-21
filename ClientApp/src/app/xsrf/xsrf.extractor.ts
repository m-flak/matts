/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 **/
import { DOCUMENT } from "@angular/common";
import { HttpXsrfTokenExtractor } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { CookieService } from "ngx-cookie-service";

@Injectable({
  providedIn: 'root',
})
export class HttpXsrfCookieExtractor implements HttpXsrfTokenExtractor {
  private lastCookieString: string = '';
  private lastToken: string | null = null;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private cookieService: CookieService
  ) { }

  getToken(): string | null {

    const cookieString = this.document.cookie || '';
    if (cookieString !== this.lastCookieString) {
      this.lastToken = this.cookieService.get('XSRF-TOKEN');
      this.lastCookieString = cookieString;
    }
    return this.lastToken;
  }
}
