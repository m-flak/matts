import { ComponentFixture, TestBed, discardPeriodicTasks, fakeAsync, tick } from '@angular/core/testing';

import { ViewListPanelComponent } from './view-list-panel.component';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { VlpProviderService } from './vlp-provider.service';
import { Component } from '@angular/core';
import { ComponentsModule } from '../components.module';
import { of } from 'rxjs';
import { By } from '@angular/platform-browser';

const TASKS = [
  {
    name: "Task 1",
  },
  {
    name: "Task 2",
  }
];

@Component({
  template: `<cmp-view-list-panel itemsName="Tasks" providerName="tasks">
    <ng-template let-task cmpTemplate="item">
      <p>{{ task.name }}</p>
    </ng-template>
    <ng-template let-task cmpTemplate="details">
      <p>{{ task.name }}</p>
    </ng-template>
  </cmp-view-list-panel>`
})
class TestComponent {}

@Component({
  template: `<cmp-view-list-panel itemsName="Tasks" [items]="tasks">
    <ng-template let-task cmpTemplate="item">
      <p>{{ task.name }}</p>
    </ng-template>
    <ng-template let-task cmpTemplate="details">
      <p>{{ task.name }}</p>
    </ng-template>
  </cmp-view-list-panel>`
})
class TestComponentNoService {
  tasks!: any[];

  ngOnInit() {
    this.tasks = [ ...TASKS ];
  }
}

describe('ViewListPanelComponent', () => {
  let component: ViewListPanelComponent;
  let componentNoService: ViewListPanelComponent;
  let testComponent: TestComponent;
  let testComponentNoService: TestComponentNoService;
  let fixture: ComponentFixture<TestComponent>;
  let fixtureNoService: ComponentFixture<TestComponentNoService>;
  let providerService: VlpProviderService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ NgbNavModule, ComponentsModule ],
      declarations: [ ViewListPanelComponent, TestComponent, TestComponentNoService ],
      providers: [ VlpProviderService ]
    })
    .compileComponents();

    providerService = TestBed.inject(VlpProviderService);
    fixture = TestBed.createComponent(TestComponent);
    fixtureNoService = TestBed.createComponent(TestComponentNoService);
    testComponent = fixture.componentInstance;
    testComponentNoService = fixtureNoService.componentInstance;

    providerService.providers.set('tasks', {
      getItemsViewData: () => of(TASKS)
    });

    fixture.detectChanges();
    await fixture.whenStable();
    fixtureNoService.detectChanges();
    await fixtureNoService.whenStable();

    component = fixture.debugElement.children[0].componentInstance;
    componentNoService = fixtureNoService.debugElement.children[0].componentInstance;
  });

  it('should create', () => {
    expect(testComponent).toBeTruthy();
    expect(testComponentNoService).toBeTruthy();
    expect(component).toBeTruthy();
    expect(componentNoService).toBeTruthy();
  });

  describe('Using the VlpProviderService', () => {
    it('should display the correct tab name in list view', () => {
      const listViewTab = fixture.debugElement.query(By.css('a.vlp-tab'));
      expect(listViewTab).toBeTruthy();
      expect(listViewTab.nativeElement.textContent).toContain(component.itemsName);
    });

    it('should display the items yielded by the provider', fakeAsync(() => {
      tick(1);
      fixture.detectChanges();
  
      const taskNames = fixture.debugElement.queryAll(By.css('p'));
      expect(taskNames).toBeTruthy();
      expect(taskNames.length).toEqual(2);
      expect(taskNames[0].nativeElement.textContent).toContain('Task 1');
      expect(taskNames[1].nativeElement.textContent).toContain('Task 2');
    }));

    it('should display the details after clicking an item', fakeAsync(() => {
      tick(1);
      fixture.detectChanges();

      const tasks = fixture.debugElement.queryAll(By.css('li'));
      expect(tasks).toBeTruthy();
      expect(tasks.length).toEqual(2);

      tasks[0].nativeElement.click();
      tick(1);
      fixture.detectChanges();

      const details = fixture.debugElement.query(By.css('div.details p'));
      expect(details).toBeTruthy();
      expect(details.nativeElement.textContent).toContain('Task 1');
    }));
  });

  describe('Not using the service', () => {
    it('should display the correct tab name in list view', () => {
      const listViewTab = fixtureNoService.debugElement.query(By.css('a.vlp-tab'));
      expect(listViewTab).toBeTruthy();
      expect(listViewTab.nativeElement.textContent).toContain(componentNoService.itemsName);
    });

    it('should display the items passed into the items property', () => {  
      const taskNames = fixtureNoService.debugElement.queryAll(By.css('p'));
      expect(taskNames).toBeTruthy();
      expect(taskNames.length).toEqual(2);
      expect(taskNames[0].nativeElement.textContent).toContain('Task 1');
      expect(taskNames[1].nativeElement.textContent).toContain('Task 2');
    });

    it('should display the details after clicking an item', fakeAsync(() => {
      const tasks = fixtureNoService.debugElement.queryAll(By.css('li'));
      expect(tasks).toBeTruthy();
      expect(tasks.length).toEqual(2);

      tasks[0].nativeElement.click();
      tick(1);
      fixtureNoService.detectChanges();

      const details = fixtureNoService.debugElement.query(By.css('div.details p'));
      expect(details).toBeTruthy();
      expect(details.nativeElement.textContent).toContain('Task 1');

      discardPeriodicTasks();
    }));
  });
});
