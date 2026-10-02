# 🚌 R.U.T.A.

### *Recomendador Urbano de Transporte Accesible*

> **R.U.T.A.** es una aplicación inteligente de navegación en transporte público diseñada para ayudar a las personas a encontrar rutas de autobús de una manera más clara, sencilla y personalizada.

---

## 📌 Sobre el proyecto

**R.U.T.A.** nace a partir de un problema común al utilizar el transporte público: muchas veces no es difícil encontrar una ruta, sino **saber cuál elegir**.

Cuando una persona necesita llegar a un destino puede encontrarse con diferentes alternativas, transbordos, paradas y recorridos. Esto puede generar confusión, especialmente cuando no conoce bien la ciudad.

R.U.T.A. busca solucionar este problema analizando diferentes alternativas de transporte y mostrando al usuario una recomendación basada en sus preferencias.

### 🎯 ¿Qué busca resolver?

La aplicación busca reducir:

* 🗺️ La dificultad para encontrar una ruta adecuada.
* 🚏 La incertidumbre sobre dónde bajar del autobús.
* 🔄 La confusión causada por los transbordos.
* 🚶 El exceso de caminata durante el recorrido.
* ⏱️ El tiempo necesario para comparar diferentes opciones.
* 🧠 La carga de información que el usuario debe procesar.

La idea principal es pasar de:

> **"Aquí están todas las rutas disponibles."**

a:

> **"Esta ruta se adapta mejor a lo que estás buscando."**

---

# ✨ Características

### 🚌 Búsqueda de rutas

El usuario puede establecer:

**Origen → Destino**

y obtener diferentes alternativas utilizando transporte público.

### 🎯 Preferencias

R.U.T.A. considera diferentes criterios para personalizar las alternativas:

* ⏱️ Tiempo de viaje
* 🔄 Número de transbordos
* 🚶 Distancia caminando
* 💰 Costo
* ⭐ Confiabilidad de la ruta

El usuario puede establecer qué factores son más importantes para su recorrido.

### 🗺️ Visualización en mapa

Las rutas se muestran mediante un mapa para facilitar la comprensión del recorrido.

La aplicación busca que el usuario pueda identificar rápidamente:

* Punto de inicio
* Paradas
* Recorrido
* Transbordos
* Punto de destino

### 🔀 Rutas alternativas

Además de una recomendación principal, el sistema puede presentar diferentes alternativas para que el usuario pueda comparar sus opciones.

### 💡 Explicación de la ruta

Una de las características principales de R.U.T.A. es que no solamente muestra una ruta.

También busca explicar **por qué una alternativa puede ser conveniente**, por ejemplo:

> Menor cantidad de transbordos, aunque el recorrido sea ligeramente más largo.

Esto permite que el usuario tome una decisión con mayor información.

---

# 🧠 ¿Cómo funciona?

R.U.T.A. representa el sistema de transporte como un **grafo**, donde diferentes elementos del transporte público pueden ser representados como nodos y conexiones.

De esta manera, el sistema puede analizar diferentes recorridos entre un origen y un destino.

### Proceso general

```text
┌───────────────┐
│    Usuario    │
└───────┬───────┘
        │
        ▼
┌─────────────────────┐
│ Origen + Destino    │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐
│ Preferencias        │
│ • Tiempo            │
│ • Transbordos       │
│ • Caminata          │
│ • Costo             │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐
│ Motor de rutas      │
│                     │
│ Grafo + RAPTOR      │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐
│ Alternativas        │
│ de transporte       │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐
│ Recomendación        │
│ personalizada        │
└─────────────────────┘
```

---

# ⚙️ Tecnologías

El proyecto utiliza diferentes tecnologías para desarrollar tanto la lógica de búsqueda como la interfaz.

| Tecnología        | Uso                                 |
| ----------------- | ----------------------------------- |
| **C#**            | Algoritmos y lógica principal       |
| **WPF**           | Interfaz de escritorio              |
| **XAML**          | Diseño de la interfaz               |
| **WebView2**      | Integración del mapa                |
| **Leaflet**       | Visualización del mapa              |
| **OpenStreetMap** | Datos cartográficos                 |
| **Git / GitHub**  | Control de versiones y colaboración |

---

# 🧮 Algoritmos

Uno de los componentes principales de R.U.T.A. es su sistema de búsqueda de rutas.

El proyecto utiliza conceptos de **teoría de grafos** y algoritmos especializados en transporte público.

### RAPTOR

R.U.T.A. utiliza **RAPTOR (Round-based Public Transit Optimized Router)** como una de las bases para analizar recorridos de transporte público.

Su objetivo es encontrar diferentes posibilidades de viaje considerando elementos como:

```text
Origen
   ↓
Ruta de autobús
   ↓
Parada
   ↓
Transbordo
   ↓
Nueva ruta
   ↓
Destino
```

Posteriormente, las alternativas pueden ser evaluadas utilizando diferentes criterios para generar recomendaciones.

---

# 🖥️ Interfaz

La interfaz está diseñada buscando mantener el proceso lo más sencillo posible.

### Flujo principal

```text
Inicio
  │
  ▼
Seleccionar origen
  │
  ▼
Seleccionar destino
  │
  ▼
Elegir preferencias
  │
  ▼
Buscar rutas
  │
  ▼
Comparar alternativas
  │
  ▼
Seleccionar recorrido
  │
  ▼
Seguir instrucciones
```

El diseño busca priorizar:

* Claridad
* Pocos pasos
* Información relevante
* Elementos visuales fáciles de identificar
* Adaptación para usuarios con diferentes niveles de experiencia tecnológica

---

# 👥 Equipo

R.U.T.A. es desarrollado por un equipo de **4 estudiantes** interesados en el desarrollo de software y en la creación de soluciones tecnológicas para problemas cotidianos.

El proyecto también representa una oportunidad para aplicar conocimientos de:

* Programación
* Desarrollo de interfaces
* Algoritmos
* Diseño UX/UI
* Bases de datos
* Gestión de proyectos
* Trabajo colaborativo

### 🧑‍💻 Equipo de desarrollo

| Integrante       | Área                                 |
| ---------------- | ------------------------------------ |
| **Adrián**       | Desarrollo, algoritmos e integración |
| **Jose Pizaña**  | Desarrollo Y Database                |
| **Yael Alexis**  | Frontend                             |
| **Edwardo Muñoz**| Frontend y diseño                    |

---

# 📚 Objetivo académico

Además de desarrollar una aplicación funcional, R.U.T.A. tiene como objetivo aplicar conocimientos adquiridos durante nuestra formación como estudiantes de desarrollo de software.

El proyecto nos permite trabajar con problemas reales y explorar conceptos como:

* Algoritmos de búsqueda
* Grafos
* Sistemas de recomendación
* Interfaces gráficas
* Mapas digitales
* Experiencia de usuario
* Desarrollo colaborativo

---

# 🚧 Alcance actual

R.U.T.A. se encuentra en desarrollo.

### Incluido

* [x] Concepto de aplicación
* [x] Definición del problema
* [x] Diseño inicial de UX/UI
* [x] Modelo de rutas
* [x] Investigación de algoritmos
* [x] Arquitectura inicial
* [ ] Motor de rutas completo
* [ ] Integración completa del mapa
* [ ] Sistema de preferencias
* [ ] Recomendación personalizada
* [ ] Pruebas con usuarios

### Fuera del alcance inicial

Para mantener el proyecto enfocado, la primera versión no contempla:

* 📍 GPS de vehículos en tiempo real
* 💳 Pagos o boletos digitales
* 🌎 Cobertura nacional
* 📶 Funcionamiento completamente offline

---

# 🗺️ Roadmap

```text
[✓] Investigación del problema
       │
       ▼
[✓] Diseño de la solución
       │
       ▼
[✓] Diseño UX/UI
       │
       ▼
[✓] Arquitectura del sistema
       │
       ▼
[ ] Desarrollo del algoritmo
       │
       ▼
[ ] Integración del mapa
       │
       ▼
[ ] Desarrollo del MVP
       │
       ▼
[ ] Pruebas con usuarios
       │
       ▼
[ ] Mejoras y optimización
```

---

# 💭 Nuestra visión

Nuestra visión es crear una herramienta que haga que utilizar el transporte público sea **menos confuso y más comprensible**, especialmente para personas que no conocen bien una ciudad.

R.U.T.A. no busca simplemente mostrar información.

Busca **convertir información compleja sobre transporte en instrucciones claras y útiles para cada usuario.**

---

# 🤝 Contribuciones

Actualmente, R.U.T.A. es un proyecto académico desarrollado por nuestro equipo.

Si el proyecto evoluciona a una versión abierta, esta sección será utilizada para establecer las reglas de contribución y colaboración.

---

# 📄 Licencia

Este proyecto fue desarrollado con fines académicos y educativos.

La información sobre licencia y distribución podrá actualizarse posteriormente.

---

<div align="center">

### 🚌 R.U.T.A.

**Haz que tu recorrido sea más fácil de entender.**

*Proyecto académico — Desarrollo de Software*

</div>
