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

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Applicant } from 'src/app/models';

@Component({
  selector: 'cmp-applicant',
  templateUrl: './applicant.component.html',
  styleUrls: ['./applicant.component.scss']
})
export class ApplicantComponent implements OnInit {
  @Input()
  pickerIndex!: number;

  @Output()
  applicantPicked: EventEmitter<number> = new EventEmitter();

  @Input()
  applicant: Applicant = {};
  
  @Input()
  isRejected: boolean = false;

  constructor() { }

  ngOnInit(): void {
  }

  onClick() {
    this.applicantPicked.emit(this.pickerIndex);
  }
}
