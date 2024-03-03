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
import { Component, Inject, InjectionToken, Input, OnInit } from '@angular/core';

export interface SideMenuItem {
  text: string;
  image: string;
  imageHint: string;
  route: string;
}
export interface SideMenuConfig {
  sections: Array<{ items: SideMenuItem[] }>;
}
export const SIDE_MENU_CONFIG = new InjectionToken<SideMenuConfig>('SideMenuConfig');

@Component({
  selector: 'app-side-menu',
  templateUrl: './side-menu.component.html',
  styleUrls: ['./side-menu.component.scss'],
})
export class SideMenuComponent implements OnInit {
  readonly _menuSections: Array<{ items: SideMenuItem[] }>;

  activeSection: number = 0;
  activeItem: number = 0;

  @Input()
  expanded: boolean = true;

  constructor(@Inject(SIDE_MENU_CONFIG) config: SideMenuConfig) {
    this._menuSections = config.sections;
  }

  ngOnInit(): void {}

  isActiveMenuItem(sectionIndex: number, itemIndex: number): boolean {
    return this.activeSection === sectionIndex && this.activeItem === itemIndex;
  }

  setActiveMenuItem(sectionIndex: number, itemIndex: number): void {
    this.activeSection = sectionIndex;
    this.activeItem = itemIndex;
  }

  collapseExpand(): void {
    this.expanded = !this.expanded;
  }

  _createIconClassName(i: number, j: number): string {
    return `side-menu__icon-s${i}-i${j}`;
  }
}
