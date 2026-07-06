import { Component } from '@angular/core';
import { Weather } from './weather/weather';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Weather],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
