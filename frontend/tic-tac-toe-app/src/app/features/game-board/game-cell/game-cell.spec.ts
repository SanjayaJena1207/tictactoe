import { TestBed } from '@angular/core/testing';
import { GameCell } from './game-cell';

describe('GameCell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameCell],
    }).compileComponents();
  });

  function createCell() {
    const fixture = TestBed.createComponent(GameCell);
    fixture.componentRef.setInput('status', 'InProgress');
    fixture.componentRef.setInput('index', 4);
    return fixture;
  }

  it('is clickable when empty and game is in progress', () => {
    const fixture = createCell();
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(true);
  });

  it('is not clickable when already filled', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('value', 'X');
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(false);
  });

  it('is not clickable when the game is no longer in progress', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Won');
    fixture.detectChanges();
    expect(fixture.componentInstance.isClickable()).toBe(false);
  });

  it('emits its own index when clicked while clickable', () => {
    const fixture = createCell();
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.componentInstance.onClick();

    expect(emitted).toEqual([4]);
  });

  it('does not emit when clicked while not clickable', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('value', 'O');
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.componentInstance.onClick();

    expect(emitted).toEqual([]);
  });

  it('is not game-over while in progress', () => {
    const fixture = createCell();
    fixture.detectChanges();
    expect(fixture.componentInstance.isGameOver()).toBe(false);
  });

  it('is game-over once the game has finished', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Won');
    fixture.detectChanges();
    expect(fixture.componentInstance.isGameOver()).toBe(true);
  });

  it('applies the game-over class to a non-winning cell once the game has ended', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Draw');
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('game-over')).toBe(true);
    expect(button.classList.contains('winning')).toBe(false);
  });

  it('does not apply the game-over class to a winning cell', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Won');
    fixture.componentRef.setInput('highlighted', true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.classList.contains('winning')).toBe(true);
    expect(button.classList.contains('game-over')).toBe(false);
  });

  it('emits its index when the rendered button is clicked while empty', () => {
    const fixture = createCell();
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.nativeElement.querySelector('button').click();

    expect(emitted).toEqual([4]);
  });

  it('does not emit when the rendered button is clicked while occupied', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('value', 'X');
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));

    fixture.nativeElement.querySelector('button').click();

    expect(emitted).toEqual([]);
  });

  it('renders the button as disabled and does not emit when clicked once the game has ended', () => {
    const fixture = createCell();
    fixture.componentRef.setInput('status', 'Won');
    fixture.detectChanges();
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);

    const emitted: number[] = [];
    fixture.componentInstance.cellClick.subscribe((index) => emitted.push(index));
    button.click();

    expect(emitted).toEqual([]);
  });
});
