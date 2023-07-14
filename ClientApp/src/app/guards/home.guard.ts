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
import { Injectable } from "@angular/core";
import { ActivatedRoute, ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { environment } from './../../environments/environment';
import { UserRoleConstants } from "../constants";

@Injectable({
    providedIn: "root",
  })
  export class HomeGuard implements CanActivate {
    constructor(private router: Router) {}

    async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
        // TODO: Actually implement instead of using env json values
        const activeRole = environment.activeRole;

        sessionStorage.setItem('MenuBar_ActiveRole', activeRole);

        if (route.data.role && route.data.role === activeRole) {
            return true;
        }

        if (state.url === '/' && activeRole == UserRoleConstants.USER_ROLE_APPLICANT) {
            return this.router.createUrlTree(['/applicant']);
        }
        else if (state.url === '/'  && activeRole == UserRoleConstants.USER_ROLE_EMPLOYER) {
            return this.router.createUrlTree(['/employer']);
        }
        if (state.url.includes('employer') && activeRole == UserRoleConstants.USER_ROLE_APPLICANT) {
            return this.router.createUrlTree(['/applicant']);
        }
        else if (state.url.includes('applicant') && activeRole == UserRoleConstants.USER_ROLE_EMPLOYER) {
            return this.router.createUrlTree(['/employer']);
        }

        return false;
    }
}
