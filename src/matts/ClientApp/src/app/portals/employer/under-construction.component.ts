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

import { Component } from "@angular/core";

@Component({
    selector: 'app-employer-ucpage',
    template: `
        <div 
            class="mat-app-background d-flex flex-column justify-content-center align-items-center p-3"
            style="border: 1px solid var(--accent-color); gap: 0.25em"
        >
            <img 
                class="w-75 h-75" 
                src="assets/under-construction_web.svg" 
                alt="Under Construction"
            />
            <h1 class="text-center">This Page is Under Construction</h1>
            <p class="text-center">We're sorry, but this page is not quite ready. Stay tuned!</p>
        </div>
    `,
    host: {
        class: 'd-flex flex-row justify-content-center align-items-center',
        style: 'width: 100%; margin-right: 1em; font-family: GTAmericaMedium !important;'
    },
})
export class UnderConstructionComponent {
    constructor() {}
}
