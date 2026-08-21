import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyerProperties } from './buyer-properties';

describe('BuyerProperties', () => {
  let component: BuyerProperties;
  let fixture: ComponentFixture<BuyerProperties>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [BuyerProperties],
    }).compileComponents();

    fixture = TestBed.createComponent(BuyerProperties);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
