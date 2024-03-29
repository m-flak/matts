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

import { Location } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Configuration } from '../models';
import { lastValueFrom, take, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private _config!: Configuration;

  get config() {
    return this._config;
  }
  set config(val: Configuration) {
    this._config = val;
  }

  get appUrl() {
    return this.baseUrl;
  }

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
  ) {}

  async loadConfig(): Promise<any> {
    return lastValueFrom(
      this.http.get<Configuration>(Location.joinWithSlash(this.baseUrl, '/api/v1/config/')).pipe(
        take(1),
        tap(c => (this._config = c)),
      ),
    );
  }
}
