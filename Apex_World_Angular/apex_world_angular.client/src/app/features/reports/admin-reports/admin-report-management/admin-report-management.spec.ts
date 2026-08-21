import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminReportManagement } from './admin-report-management';

describe('AdminReportManagement', () => {
  let component: AdminReportManagement;
  let fixture: ComponentFixture<AdminReportManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AdminReportManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminReportManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
