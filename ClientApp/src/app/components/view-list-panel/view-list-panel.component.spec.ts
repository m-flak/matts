import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewListPanelComponent } from './view-list-panel.component';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';

describe('ViewListPanelComponent', () => {
  let component: ViewListPanelComponent;
  let fixture: ComponentFixture<ViewListPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ NgbNavModule],
      declarations: [ ViewListPanelComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewListPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
