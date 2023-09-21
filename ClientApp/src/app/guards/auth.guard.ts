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
import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
  CanMatchFn,
  Route,
  UrlSegment,
  CanMatch,
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate, CanMatch {
  constructor(
    private router: Router,
    private authService: AuthService,
  ) {}

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
    return await this._doCheck(() => this.router.createUrlTree(['/welcome/login']));

    // // Token is in local storage but no login was performed thru interaction
    // if (!this.authService.isLoggedIn() && this.authService.hasToken()) {
    //   try {
    //     const _ = await lastValueFrom(this.authService.loginUser());
    //     return true;
    //   } catch (e) {
    //     console.error(e);
    //     return this.router.createUrlTree(['/welcome/login']);
    //   }
    // } else if (!this.authService.isLoggedIn()) {
    //   return this.router.createUrlTree(['/welcome/login']);
    // }

    // return true;
  }

  async canMatch(route: Route, segments: UrlSegment[]): Promise<boolean | UrlTree> {
    return await this._doCheck(() => false);
  }

  private async _doCheck(failCb: () => boolean | UrlTree) {
    // Token is in local storage but no login was performed thru interaction
    if (!this.authService.isLoggedIn() && this.authService.hasToken()) {
      try {
        const _ = await lastValueFrom(this.authService.loginUser());
        return true;
      } catch (e) {
        console.error(e);
        return failCb();
      }
    } else if (!this.authService.isLoggedIn()) {
      return failCb();
    }

    return true;
  }
}
