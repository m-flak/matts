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
import { AfterViewInit, Component, ElementRef, HostBinding, Input, OnInit, ViewChild } from '@angular/core';
import random from 'lodash.random';

@Component({
  selector: 'cmp-avatar',
  templateUrl: './avatar.component.html',
  styleUrls: ['./avatar.component.scss']
})
export class AvatarComponent implements OnInit, AfterViewInit {
  @ViewChild('firstInitial')
  firstInitialText!: ElementRef | null;

  @HostBinding('style.--first-initial-width')
  firstInitialWidth = '1px';

  @Input()
  avatarImage = '';

  @Input()
  avatarName = '';

  @Input()
  randomizeColor = false;

  @Input()
  colorIndex = 0; // scss index starts at 1
  
  constructor() { }

  ngOnInit(): void {
    if (this.randomizeColor) {
      this.randomizeColors();
    }
  }

  ngAfterViewInit(): void {
    if (this.avatarImage === '' && this.firstInitialText !== null) {
      // prevents change detector error
      setTimeout(() => {
        this.firstInitialWidth = window.getComputedStyle(this.firstInitialText!.nativeElement)?.getPropertyValue('width');
      });
    }
  }

  randomizeColors(): void {
    this.colorIndex = random(1, 8, false);
  }

  _createOverrideCircleFill(): string | null {
    if (this.colorIndex < 1 && !this.randomizeColor) {
      return null;                           // neither index or 2nd pass randomization enabled. Use defaults.
    }
    else if (this.colorIndex < 1 && this.randomizeColor) {
      this.randomizeColors();                // 2nd pass randomization
    }

    return `var(--avatar-color-circle-${this.colorIndex}) !important`;
  }

  _createOverrideTextFill(): string | null {
    if (this.colorIndex < 1 && !this.randomizeColor) {
      return null;                           // neither index or 2nd pass randomization enabled. Use defaults.
    }
    else if (this.colorIndex < 1 && this.randomizeColor) {
      this.randomizeColors();                // 2nd pass randomization
    }

    return `var(--avatar-color-text-${this.colorIndex}) !important`;
  }
}
