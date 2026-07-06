import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherService, WeatherForecast } from '../weather.service';

@Component({
  selector: 'app-weather',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './weather.html',
  styleUrl: './weather.css',
})
export class Weather implements OnInit {
  forecasts: WeatherForecast[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(private weatherService: WeatherService) {}

  ngOnInit(): void {
    this.fetchWeather();
  }

  fetchWeather(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.forecasts = [];

    this.weatherService.getForecasts().subscribe({
      next: (data) => {
        this.forecasts = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching weather data', error);
        this.errorMessage = 'Failed to retrieve weather forecasts. The service might be temporarily unavailable.';
        this.isLoading = false;
      },
    });
  }
}
