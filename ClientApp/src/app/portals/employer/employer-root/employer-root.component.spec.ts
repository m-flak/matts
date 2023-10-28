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
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmployerRootComponent } from './employer-root.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { RouterTestingModule } from '@angular/router/testing';

describe('EmployerRootComponent', () => {
  let component: EmployerRootComponent;
  let fixture: ComponentFixture<EmployerRootComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule.withRoutes([]),
        ComponentsModule
      ],
      declarations: [ EmployerRootComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmployerRootComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});