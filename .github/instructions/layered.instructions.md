# Arquitectura en capas

Este proyecto sigue una arquitectura en capas, que separa las responsabilidades en diferentes niveles para mejorar la mantenibilidad y escalabilidad del código. A continuación se describen las capas principales:

1. **Capa de Presentación (UI)**: Esta capa es responsable de la interacción con el usuario. Incluye componentes como handlers de endpoint HTTP. Su función principal es recibir las entradas del usuario y mostrar los resultados.

Se guarda en la carpeta `presentation`.

2. **Capa de Negocio (Lógica de Negocio)**: Esta capa contiene la lógica de negocio de la aplicación. Aquí es donde se procesan las solicitudes recibidas de la capa de presentación, se aplican las reglas de negocio y se coordinan las operaciones entre la capa de presentación y la capa de persistencia. Se crean clases de tipo Service para encapsular esta lógica.

Se guarda en la carpeta `business`.

3. **Capa de Persistencia (Acceso a Datos)**: Esta capa se encarga de la gestión y almacenamiento de datos. Por ahora utiliza repositorios en memoria para almacenar los datos, pero en el futuro podría integrarse con bases de datos u otros sistemas de almacenamiento. Las clases de tipo Repository se encuentran en esta capa.

Se guarda en la carpeta `persistence`.